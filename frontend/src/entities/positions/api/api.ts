import { apiClient } from "@/shared/api/axios-instance";
import {
	Envelope,
	envelopeInfinityQueryOptions,
	PaginationEnvelope,
} from "@/shared/api/envelops";
import { infiniteQueryOptions } from "@tanstack/react-query";
import { PositionDto } from "../model/types";
import { CreatePositionRequest, GetPositionsRequest } from "./types";

const POSITION_KEY = "positions";
const POSITION_ENDPOINT = "/positions";

export const positionsApi = {
	baseKey: POSITION_KEY,

	getPositions: async (request: GetPositionsRequest) => {
		const response = await apiClient.get<
			Envelope<PaginationEnvelope<PositionDto>>
		>(POSITION_ENDPOINT, {
			params: request,
		});

		return response.data;
	},

	getPositionsInfiniteQueryOptions: (request: GetPositionsRequest) => {
		return infiniteQueryOptions({
			queryKey: [positionsApi.baseKey, request],
			queryFn: ({ pageParam }) => {
				return positionsApi.getPositions({ ...request, page: pageParam });
			},
			...envelopeInfinityQueryOptions<PositionDto>(),
		});
	},

	createPosition: async (request: CreatePositionRequest) => {
		const response = await apiClient.post<Envelope<PositionDto>>(
			POSITION_ENDPOINT,
			request,
		);
		return response.data;
	},
};
