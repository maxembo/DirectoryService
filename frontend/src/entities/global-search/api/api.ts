import { apiClient, PaginationEnvelope, type Envelope } from "@/shared/api";
import { GlobalSearchDto } from "../model/types";
import { queryOptions } from "@tanstack/react-query";

const SEARCH_KEY = "search";
const SEARCH_ENDPOINT = "/search";

export const globalSearchApi = {
	baseKey: SEARCH_KEY,

	getGlobalSearchList: async (q: string) => {
		const response = await apiClient.get<
			Envelope<PaginationEnvelope<GlobalSearchDto>>
		>(`${SEARCH_ENDPOINT}`, { params: { q } });

		return response.data;
	},

	getGlobalSearchListQueryOptions: (q: string) =>
		queryOptions({
			queryKey: [globalSearchApi.baseKey, q],
			queryFn: () => globalSearchApi.getGlobalSearchList(q),
		}),
};
