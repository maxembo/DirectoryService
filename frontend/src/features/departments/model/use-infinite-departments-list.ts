import { departmentsApi } from "@/entities/departments/api/api";
import { GetDepartmentsInfinityRequest } from "@/entities/departments/api/types";
import {
	DepartmentListId,
	useDepartmentIsActive,
	useDepartmentIsParent,
	useDepartmentParentId,
	useDepartmentSearch,
	useDepartmentSelectedLocations,
	useDepartmentSortBy,
	useDepartmentSortDirection,
} from "@/features/departments/model/department-list-store";
import { EnvelopeError } from "@/shared/api/errors";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";

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
			selectedLocations: selectedLocations.map((location) => location.id),
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
