import { gql } from "@apollo/client";

export const SEND_MESSAGE = `
mutation SendMessage($message: String!, $from: Int!, $to: Int!) {
  _dbMutations {
    _chatMutations {
      sendMessage(chatMessageInput: { message: $message, from: $from, to: $to })
    }
  }
}
`;
