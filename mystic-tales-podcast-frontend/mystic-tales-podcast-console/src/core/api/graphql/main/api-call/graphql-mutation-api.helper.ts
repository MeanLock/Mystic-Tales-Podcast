import { ApolloClient, gql, ApolloQueryResult, FetchResult } from "@apollo/client";
import { MessResponse, response_with_mess } from "../response-generator/v2/response-generator";
import { AppError } from "../errors";


interface GraphQLApiHelperParams {
    client: ApolloClient<any>;
    mutation: string;
    variables?: object;
}



export async function callGraphQLMutation(
    { client, mutation, variables }: GraphQLApiHelperParams,
    title?: string
): Promise<MessResponse> {
    const mess_title = title || null;
    const request_variables = variables || {};
    try {
        const response: ApolloQueryResult<any> | FetchResult<any> = await client.mutate({
            mutation: gql`${mutation}`,
            variables: request_variables,
        });
        return response_with_mess(
            true,
            false,
            mess_title,
            "Request thành công",
            response.data
        );
    } catch (error: any) {
        console.error("API call error:", error);

        let isAppError = false;
        let errorMessage = "Có lỗi xảy ra";

        // 🔍 Lấy ra lỗi gốc nếu có (được ném từ ApolloLink)
        const originalError = error?.originalError || error?.cause || error;

        if (originalError instanceof AppError) {
            isAppError = true;
            errorMessage = originalError.message;
        }
        else if (error.networkError) {
            isAppError = false;
            errorMessage = `Network error: ${error.networkError.message}`;
        }
        else if (error.graphQLErrors?.length > 0) {
            isAppError = false;
            errorMessage = error.graphQLErrors[0].message || "GraphQL error";
        }
        else {
            isAppError = true;
            errorMessage = error.message || "Unexpected error";
        }

        return response_with_mess(
            false,
            isAppError,
            mess_title,
            errorMessage,
            null
        );
    }

}