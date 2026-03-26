import { apiClient } from "@/shared/api/api-instance";
import { PaginationEnvelope } from "@/shared/types";
import { AddressDto, Location } from "./types";

export interface GetLocationsRequest extends PaginationRequest {
	departmentIds?: string[];
	search?: string;
	isActive?: boolean;
	sortBy?: string;
	sortDirection?: string;
}

export interface PaginationRequest {
	page?: number;
	pageSize?: number;
}

export type CreateLocationRequest = {
	name: string;
	address: AddressDto;
	timezone: string;
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
	getLocations: async (request: GetLocationsRequest) => {
		const response = await apiClient.get<
			Envelope<PaginationEnvelope<Location>>
		>("/locations", {
			params: request,
		});

		return response.data.result;
	},

	createLocation: async (request: CreateLocationRequest) => {
		const response = await apiClient.post<Envelope<Location>>(
			"/locations",
			request,
		);

		return response.data.result;
	},
};
