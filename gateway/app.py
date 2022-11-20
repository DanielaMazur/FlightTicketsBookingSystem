from flask import Flask
from flask import request, Response
from enum import Enum
import requests
from datetime import datetime, timedelta
import urllib3
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

class ServiceNames(Enum):
    AdminService = 'AdminService'
    CustomerService = 'CustomerService'
    PaymentService = 'PaymentService'
    AuthService = "AuthService"

app = Flask(__name__)

services = []
CIRCUIT_BREAKER_MAX_ERRORS = 3
CIRCUIT_BREAKER_THRESHOLD = 100 #seconds

@app.route("/")
def gateway():
    return "<p>Gateway is up!</p>"

@app.route("/registerService", methods = ['POST'])
def registerService():
    data = request.json
    services.append(data)
    print(data)
    return 'OK'

@app.route("/login", methods = ['POST'])
def login():
    filteredServices = filter(lambda s: s["Name"] == ServiceNames.AuthService.value, services)
    serviceInstances = list(filteredServices)
    if len(serviceInstances) == 0:
        print("No", ServiceNames.AuthService.value, "found")
        return 'Error'
    service = serviceInstances[0]
    headers = {"Content-Type": "application/x-www-form-urlencoded"} 
    r = requests.post(service["Url"] + "/connect/token", request.form, headers = headers, verify=False)        

    return Response(
            r.text,
            status=r.status_code,
            content_type=r.headers['content-type'])

counter = 0

@app.route("/tickets", methods = ['POST', 'GET'])
def getTickets():
    global counter
    if request.method == 'GET':
        filteredServices = filter(lambda s: s["Name"] == ServiceNames.CustomerService.value, services)
        serviceInstances = list(filteredServices)
        if len(serviceInstances) == 0:
            print("No", ServiceNames.CustomerService.value, "found")
            return 'Error'

        # Round robin logic
        service = serviceInstances[counter % len(serviceInstances)]
        counter += 1
        serviceIndex = services.index(service)

        try:
            r = requests.get(service["Url"] + "/tickets", verify=False)
        except:
            circuit_breaker(serviceIndex)
            return Response(
                    "Internal Server Error",
                    status=500)

        if r.status_code >= 500:
            circuit_breaker(serviceIndex)

        return Response(
            r.text,
            status=r.status_code,
            content_type=r.headers['content-type'])

    if request.method == 'POST':
        filteredServices = filter(lambda s: s["Name"] == ServiceNames.AdminService.value, services)
        serviceInstances = list(filteredServices)
        if len(serviceInstances) == 0:
            print("No", ServiceNames.AdminService.value, "found")
            return 'Error'
        service = serviceInstances[0]
        r = requests.post(service["Url"] + "/tickets", json=request.json, headers = request.headers, verify=False)

        return Response(
            r.text,
            status=r.status_code)
            
@app.route("/tickets/<id>", methods = ['PATCH', 'DELETE'])
def manageTickets(id):
    if request.method == "PATCH":
        filteredServices = filter(lambda s: s["Name"] == ServiceNames.AdminService.value, services)
        serviceInstances = list(filteredServices)
        if len(serviceInstances) == 0:
            print("No", ServiceNames.AdminService.value, "found")
            return 'Error'
        service = serviceInstances[0]
        r = requests.patch(service["Url"] + "/tickets/" + id, json=request.json, headers = request.headers, verify=False)
        return Response(
            r.text,
            status=r.status_code)

    if request.method == "DELETE":
        filteredServices = filter(lambda s: s["Name"] == ServiceNames.AdminService.value, services)
        serviceInstances = list(filteredServices)
        if len(serviceInstances) == 0:
            print("No", ServiceNames.AdminService.value, "found")
            return 'Error'
        service = serviceInstances[0]
        r = requests.delete(service["Url"] + "/tickets/" + id, verify=False)
        return Response(
            r.text,
            status=r.status_code)

def circuit_breaker(serviceIndex):
    global services
    service = services[serviceIndex]

    if "Errors" in service:
        if service["ErrorsLastUpdate"] + timedelta(seconds=CIRCUIT_BREAKER_THRESHOLD) < datetime.now():
            print("RESET ERRORS, THRESHOLD EXCEDED")
            service["Errors"] = 1
        else:
            print("INCREMENT ERRORS")
            service["Errors"] += 1
        service["ErrorsLastUpdate"] = datetime.now()
    else:
        service["Errors"] = 1
        service["ErrorsLastUpdate"] = datetime.now()

    if service["Errors"] > CIRCUIT_BREAKER_MAX_ERRORS:
        print("CIRCUIT BREAKER MAX ERRORS EXCEDED FOR SERVICE ", service["Name"])
        services.pop(serviceIndex)
        return
    
    print("REQUEST FAILED", service)
    
    services[serviceIndex] = service    