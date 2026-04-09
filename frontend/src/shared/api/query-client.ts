import { QueryClient } from "@tanstack/react-query";

const STALE_TIME = 60 * 1000;

export const queryClient = new QueryClient({
	defaultOptions: {
		queries: {
			staleTime: STALE_TIME,
		},
	},
});
