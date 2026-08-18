import {
	departmentsApi,
	type GetDepartmentChildrenRequest,
} from "@/entities/departments";
import { EnvelopeError } from "@/shared/api/errors";
import { useInfiniteQuery } from "@tanstack/react-query";

type Props = {
	request: GetDepartmentChildrenRequest;
	enabled: boolean;
};

export function useInfiniteDepartmentChildren({ request, enabled }: Props) {
	const {
		data,
		isLoading,
		isError,
		error,
		isFetchingNextPage,
		isFetchNextPageError,
		hasNextPage,
		fetchNextPage,
		refetch,
	} = useInfiniteQuery({
		...departmentsApi.getDepartmentChildrenInfinityOptions({ ...request }),
		enabled: enabled,
	});

	return {
		departmentChildren: data?.result?.items ?? [],
		isLoading,
		isError,
		errorMessage:
			error instanceof EnvelopeError
				? error.message
				: "Не удалось загрузить подразделения",
		isFetchingNextPage,
		isFetchNextPageError,
		hasNextPage,
		fetchNextPage,
		refetch,
	};
}
