from dotenv import load_dotenv
from os import getenv
load_dotenv()

API_SERVER = "http://localhost:5182/api/"
API_KEY = getenv("API_KEY")
