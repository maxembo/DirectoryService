import {
	type GetDepartmentsInfinityRequest,
	departmentsApi,
} from "@/entities/departments";
import { EnvelopeError } from "@/shared/api/errors";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";
import {
	useDepartmentIsActive,
	useDepartmentIsParent,
	useDepartmentParentId,
	useDepartmentSearch,
	useDepartmentSelectedLocations,
	useDepartmentSortBy,
	useDepartmentSortDirection,
	type DepartmentListId,
} from "./department-list-store";

type Props = {
	stateId?: DepartmentListId;
	request?: GetDepartmentsInfinityRequest;
};

export function useInfiniteDepartmentsList({ stateId, request }: Props) {
	const selectedLocations = useDepartmentSelectedLocations(stateId);
	const search = useDepartmentSearch(stateId);
	const [debouncedSearch] = useDebounce(search, 600);
	const isParent = useDepartmentIsParent(stateId);
	const parentId = useDepartmentParentId(stateId);
	const isActive = useDepartmentIsActive(stateId);
	const sortBy = useDepartmentSortBy(stateId);
	const sortDirection = useDepartmentSortDirection(stateId);

	const {
		data,
		isPending,
		isError,
		error,
		hasNextPage,
		isFetchingNextPage,
		fetchNextPage,
	} = useInfiniteQuery({
		...departmentsApi.getDepartmentsInfinityQueryOptions({
			locationsIds: selectedLocations,
			search: debouncedSearch,
			isParent: isParent === "all" ? undefined : isParent === "parent",
			isActive: isActive === "all" ? undefined : isActive === "active",
			sortBy,
			sortDirection,
			parentId,
			...request,
		}),
	});

	const cursorRef = useCursorRef({
		hasNextPage,
		isFetchingNextPage,
		fetchNextPage,
	});

	return {
		departments: data?.result?.items ?? [],
		isPending,
		isError,
		error: error instanceof EnvelopeError ? error : undefined,
		isFetchingNextPage,
		cursorRef,
	};
}
