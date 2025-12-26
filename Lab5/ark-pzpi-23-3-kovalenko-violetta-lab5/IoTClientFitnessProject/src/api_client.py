from typing import Dict, List, Optional
from config_manager import ConfigManager
from base_api_client import BaseApiClient
from telemetry_api_client import TelemetryApiClient
from diet_api_client import DietApiClient
from statistics_api_client import StatisticsApiClient
from user_api_client import UserApiClient
from recommendations_api_client import RecommendationsApiClient


class ApiClient(TelemetryApiClient, DietApiClient, StatisticsApiClient, 
                UserApiClient, RecommendationsApiClient):
    
    def __init__(self, config_manager: ConfigManager):
        BaseApiClient.__init__(self, config_manager)
