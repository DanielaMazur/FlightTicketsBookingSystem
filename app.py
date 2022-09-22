from flask import Flask

app = Flask(__name__)

@app.route("/")
def gateway():
    return "<p>Gateway is up!</p>"