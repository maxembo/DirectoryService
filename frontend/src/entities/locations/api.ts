import { apiClient } from "@/shared/api/axios-instance";
import { Envelope, PaginationEnvelope } from "@/shared/api/envelops";
import { SortByFilter, SortDirectionFilter } from "@/shared/api/filters";
import { PaginationRequest } from "@/shared/api/pagination-request";
import { queryOptions } from "@tanstack/react-query";
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
		return response.data;
	},
};
