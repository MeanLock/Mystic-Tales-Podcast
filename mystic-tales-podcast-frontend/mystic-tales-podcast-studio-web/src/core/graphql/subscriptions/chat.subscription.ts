import { gql } from "@apollo/client";

export const CHAT_ROOMS_SUBSCRIPTION = `
subscription OnChatRoomListUpdate($accountId: Int!) {
    onChatRoomListUpdate(accountId: $accountId) {
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


`;

export const MESSAGE_RECEIVED_SUBSCRIPTION = `
subscription OnMessageSent($accountId: Int!) {
    onMessageSent(accountId: $accountId) {
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

`;
