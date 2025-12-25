from typing import Dict, List, Optional
from base_api_client import BaseApiClient


class RecommendationsApiClient(BaseApiClient):
    
    def get_recommendations(self, user_id: Optional[int] = None) -> List[Dict]:
        if user_id is None:
            user_id = self.user_id
        
        url = f"{self.base_url}/api/recommendations/corrections"
        params = {"userId": user_id}
        
        response = self._make_request('GET', url, params=params)
        
        if response:
            return response.json()
        return []

