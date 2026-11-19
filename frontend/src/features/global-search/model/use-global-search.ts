import { globalSearchApi } from "@/entities/global-search";
import { useQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";

type Props = {
	q: string;
};
export function useGlobalSearch({ q }: Props) {
	const [debouncedGlobalSearch] = useDebounce(q, 400);

	const isDebouncing = q !== debouncedGlobalSearch;

	const { isSuccess, data, error, isLoading, isError } = useQuery({
		...globalSearchApi.getGlobalSearchListQueryOptions(debouncedGlobalSearch),
		enabled: !isDebouncing && debouncedGlobalSearch.trim().length >= 2,
	});

	return {
		globalSearchList: data?.result?.items ?? [],
		error,
		isLoading,
		isSuccess,
		isError,
		isDebouncing,
	};
}
