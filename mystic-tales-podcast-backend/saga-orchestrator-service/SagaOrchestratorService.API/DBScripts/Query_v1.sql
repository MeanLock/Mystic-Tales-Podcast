Select flowName, initialData, resultData, flowStatus, createdAt from SagaInstance order by createdAt DESC
Select sagaInstanceId , stepName, stepStatus, requestData, responseData from SagaStepExecution order by createdAt DESC


{    "AccountId": 17,    "PodcastShowId": "172eb07f-2121-4ff7-8b5c-91eeec0dee86"  }
{    "ErrorMessage": "Submit podcast episode audio file failed, error: HashStream does not support seeking"  }
{    "ErrorMessage": "Process podcast episode publish audio failed, error: HLS processing failed: HashStream does not support seeking"  }
{    "PodcastChannelId": "454c5bb9-217a-4532-a271-c19b2223b9c7",    "PodcasterId": 17,    "DmcaDismissedShowIds": [],    "DmcaDismissedEpisodeIds": []  }


{    "ErrorMessage": "Submit podcast episode audio file failed, error: Error while transcribing audio in BusinessLogic"  }

complete-all-user-booking-producing-listen-sessions

BE2EF4BC-3DB7-4C77-AF39-B05F89BDD50C	send-subscription-service-email	FAILED	{    "SendSubscriptionServiceEmailInfo": {      "MailTypeName": "PodcastSubscriptionRegistration",      "ToEmail": "vuthif@email.com",      "MailObject": {        "CustomerFullName": "Bố Lộc Vĩ Đại",        "Price": 9996.0,        "CreatedDate": "2025-11-28T17:39:23.1749828"      }    }  }	{    "ErrorMessage": "Send email failed, error: Gửi mail thất bại"  }