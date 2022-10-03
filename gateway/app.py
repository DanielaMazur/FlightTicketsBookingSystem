from flask import Flask
from flask import request, Response
from enum import Enum
import requests

class ServiceNames(Enum):
    AdminService = 'AdminService'
    CustomerService = 'CustomerService'
    PaymentService = 'PaymentService'
    AuthService = "AuthService"

app = Flask(__name__)

services = []

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

@app.route("/tickets", methods = ['POST', 'GET'])
def getTickets():
    if request.method == 'GET':
        filteredServices = filter(lambda s: s["Name"] == ServiceNames.CustomerService.value, services)
        serviceInstances = list(filteredServices)
        if len(serviceInstances) == 0:
            print("No", ServiceNames.CustomerService.value, "found")
            return 'Error'
        # Round robin logic will be implemented here
        service = serviceInstances[0]
        r = requests.get(service["Url"] + "/tickets", verify=False)

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

