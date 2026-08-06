import { apiClient } from "@/shared/api/axios-instance";
import {
	Envelope,
	envelopeInfinityQueryOptions,
	PaginationEnvelope,
} from "@/shared/api/envelops";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import { Location, LocationId } from "../model/types";
import {
	CreateLocationRequest,
	GetLocationsInfinityRequest,
	GetLocationsRequest,
	UpdateLocationRequest,
} from "./types";

const LOCATIONS_KEY = "locations";
const LOCATIONS_ENDPOINT = "/locations";

export const locationsApi = {
	baseKey: LOCATIONS_KEY,

	getLocations: async (request: GetLocationsRequest) => {
		const response = await apiClient.get<
			Envelope<PaginationEnvelope<Location>>
		>(LOCATIONS_ENDPOINT, { params: request });

		return response.data;
	},

	getLocationsQueryOptions: (request: GetLocationsRequest) =>
		queryOptions({
			queryKey: [locationsApi.baseKey, request],
			queryFn: () => {
				return locationsApi.getLocations(request);
			},
		}),

	getLocationsInfinityQueryOptions: (request: GetLocationsInfinityRequest) => {
		return infiniteQueryOptions({
			queryKey: [locationsApi.baseKey, request],
			queryFn: ({ pageParam }) => {
				return locationsApi.getLocations({ ...request, page: pageParam });
			},
			...envelopeInfinityQueryOptions<Location>(),
		});
	},

	createLocation: async (request: CreateLocationRequest) => {
		const response = await apiClient.post<Envelope<Location>>(
			LOCATIONS_ENDPOINT,
			request,
		);

		return response.data;
	},

	updateLocation: async (
		request: UpdateLocationRequest & { locationId: LocationId },
	) => {
		const response = await apiClient.patch<Envelope<Location>>(
			`${LOCATIONS_ENDPOINT}/${request.locationId}`,
			request,
		);

		return response.data;
	},

	restoreLocation: async (locationId: LocationId) => {
		const response = await apiClient.patch<Envelope<LocationId>>(
			`${LOCATIONS_ENDPOINT}/${locationId}/restore`,
		);

		return response.data;
	},

	deleteLocation: async (locationId: LocationId) => {
		const response = await apiClient.delete<Envelope<LocationId>>(
			`${LOCATIONS_ENDPOINT}/${locationId}`,
		);

		return response.data;
	},
};
