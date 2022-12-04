from flask import Flask
from flask import request, Response
from enum import Enum
import requests
from datetime import datetime, timedelta
import urllib3
import json
import logging
import sys
from python_elastic_logstash import ElasticHandler, ElasticFormatter
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

logging.basicConfig()

logger = logging.getLogger('gateway')
logger.setLevel(logging.DEBUG)

elasticsearch_endpoint = 'http://elasticsearch:9200'

elastic_handler = ElasticHandler(elasticsearch_endpoint)
elastic_handler.setFormatter(ElasticFormatter())

logger.addHandler(elastic_handler)

class ServiceNames(Enum):
    AdminService = 'AdminService'
    CustomerService = 'CustomerService'
    CacheService = 'CacheService'
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
    logger.debug(data)
    return 'OK'

@app.route("/login", methods = ['POST'])
def login():
    filteredServices = filter(lambda s: s["Name"] == ServiceNames.AuthService.value, services)
    serviceInstances = list(filteredServices)
    if len(serviceInstances) == 0:
        logger.debug("No " + ServiceNames.AuthService.value + " found")
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
        cacheData = cache_request("tickets", get_cache_data)
        if cacheData and cacheData.status_code == 200:
            return Response(
                cacheData.text,
                status=cacheData.status_code,
                content_type=cacheData.headers['content-type'])

        filteredServices = filter(lambda s: s["Name"] == ServiceNames.CustomerService.value, services)
        serviceInstances = list(filteredServices)
        if len(serviceInstances) == 0:
            logger.debug("No " + ServiceNames.CustomerService.value + " found")
            return 'Error'

        # Round robin logic
        service = serviceInstances[counter % len(serviceInstances)]
        counter += 1
        serviceIndex = services.index(service)

        try:
            r = requests.get(service["Url"] + "/tickets", verify=False)
            body = {
                "key":"tickets",
                "cache": r.json()};

            logger.debug("body:")
            logger.debug(body)

            cache_request("tickets", store_cache_data(body))
        except Exception as e:
            logger.error('Failed to GET tickets '+ str(e))
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
            logger.debug("No " + ServiceNames.AdminService.value + " found")
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
            logger.debug("No " + ServiceNames.AdminService.value + " found")
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
            logger.debug("No " + ServiceNames.AdminService.value + " found")
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
            logger.debug("RESET ERRORS, THRESHOLD EXCEDED")
            service["Errors"] = 1
        else:
            logger.debug("INCREMENT ERRORS")
            service["Errors"] += 1
        service["ErrorsLastUpdate"] = datetime.now()
    else:
        service["Errors"] = 1
        service["ErrorsLastUpdate"] = datetime.now()

    if service["Errors"] > CIRCUIT_BREAKER_MAX_ERRORS:
        logger.debug("CIRCUIT BREAKER MAX ERRORS EXCEDED FOR SERVICE " + service["Name"])
        services.pop(serviceIndex)
        return
    
    logger.debug("REQUEST FAILED " + service["Name"])
    
    services[serviceIndex] = service    

def cache_request(key, request):
    filteredServices = filter(lambda s: s["Name"] == ServiceNames.CacheService.value, services)
    serviceInstances = list(filteredServices)
    if len(serviceInstances) == 0:
        logger.debug("No " + ServiceNames.CacheService.value + " found")
        return False
    
    currentServiceIndex = 0
    logger.info("Current Service Index: " + str(currentServiceIndex))
    r = request(serviceInstances[currentServiceIndex])
    while not r and currentServiceIndex < len(serviceInstances) - 1:
        currentServiceIndex += 1
        r = request(serviceInstances[currentServiceIndex])

    return r

def get_cache_data(service):
    logger.info("GET cache data from: " + service["Url"] + "/cache/tickets")
    try:
        r = requests.get(service["Url"] + "/cache/tickets", verify=False)
    except:
        logger.info("Exception during GET cache data happened")
        return False

    if r.status_code >= 500:
        logger.info("Cache Service returned an error with status code bigger than 500")
        return False
    
    return r

def store_cache_data(data):
    logger.info("Store cache data")
    logger.info(data)
    def post_cache_data(service):
        try:
            logger.info("Request to store cache data")
            r = requests.post(service["Url"] + "/cache", json=data, verify=False)
        except Exception as e:
            logger.info("Exception during store cache data " + str(e))
            return False

        if r.status_code >= 500:
            logger.info("Cache Service returned an error with status code bigger than 500")
            return False
        
        return r
    return post_cache_data