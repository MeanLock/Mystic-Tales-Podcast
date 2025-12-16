import { gql } from "@apollo/client";

export const CHAT_ROOMS_BY_ACCOUNT_ID = `
query chatRoomsByAccountId($accountId: Int!) { 
    _dbQueries {
        _chatQueries {
            chatRoomsByAccountId(accountId: $accountId) {
                id
                account {
                    id
                    imageUrl
                    email
                    fullName
                    roleId
                    jobTypeId
                    dateOfBirth
                    address
                    phone
                    isDeactivated
                    createdAt
                }
                lastChatMessage {
                    id
                    message
                    created
                    senderName
                    from
                    to
                }
            }
        }
    }
}
`;

export const CHAT_MESSAGES_BY_1V1 = `
query chatMessagesBy1v1($accountId1: Int!, $accountId2: Int!) { 
    _dbQueries {
        _chatQueries {
            chatMessagesBy1v1(accountId1: $accountId1, accountId2: $accountId2) {
                message
                created
                senderName
                from
                to
                fromAccount {
                    id
                    imageUrl
                    fullName
                }
                toAccount {
                    id
                    imageUrl
                    fullName
                }
            }
        }
    }
}`;
