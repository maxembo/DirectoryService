import { apiClient } from "@/shared/api/api-instance";
import { Location } from "./types";

export type GetLocationsRequest = {
	search?: string;
	page: number;
	pageSize: number;
};

export type Envelope<T = unknown> = {
	result: T | null;
	error: ApiError | null;
	isError: boolean;
	timeGenerated: string;
};

export type ApiError = {
	messages: ErrorMessage[];
	type: ErrorType;
};

export type ErrorMessage = {
	code: string;
	message: string;
	invalidField?: string | null;
};

export type ErrorType = "validation" | "not_found" | "failure" | "conflict";

export const locationsApi = {
	getLocations: async (request: GetLocationsRequest): Promise<Location[]> => {
		const response = await apiClient.get<Envelope<{ items: Location[] }>>(
			"/locations",
			{
				params: request,
			},
		);

		return response.data.result?.items || [];
	},
};
