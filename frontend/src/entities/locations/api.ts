import { apiClient } from "@/shared/api/api-instance";
import { PaginationRequest } from "@/shared/api/pagination-request";
import { PaginationEnvelope } from "@/shared/types";
import { queryOptions } from "@tanstack/react-query";
import {
	SortByFilter,
	SortDirectionFilter,
} from "./hooks/use-location-filters";
import { AddressDto, Location } from "./types";

export interface GetLocationsRequest extends PaginationRequest {
	departmentIds?: string[];
	search?: string;
	isActive?: boolean;
	sortBy?: SortByFilter;
	sortDirection?: SortDirectionFilter;
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
	baseKey: "locations",

	getLocationsQueryOptions: (request: GetLocationsRequest) =>
		queryOptions({
			queryKey: [locationsApi.baseKey, request],
			queryFn: async ({ signal }) => {
				const response = await apiClient.get<
					Envelope<PaginationEnvelope<Location>>
				>("/locations", {
					params: request,
					signal,
				});
				return response.data;
			},
		}),

	createLocation: async (request: CreateLocationRequest) => {
		const response = await apiClient.post<Envelope<Location>>(
			"/locations",
			request,
		);

		return response.data.result;
	},
};
