import type { DepartmentTreeDto } from "@/entities/departments";
import {
	DepartmentTreeId,
	setDepartmentTreeSelectedId,
	toggleDepartmentTreeExpandedId,
	useDepartmentTreeExpandedIds,
	useDepartmentTreeSelectedId,
} from "./department-tree-store";
import { useInfiniteDepartmentChildren } from "./use-infinite-department-children";

type Props = {
	department: DepartmentTreeDto;
	stateId?: DepartmentTreeId;
};

export function useDepartmentTreeNode({ department, stateId }: Props) {
	const selectedId = useDepartmentTreeSelectedId(stateId);
	const expandedIds = useDepartmentTreeExpandedIds(stateId);

	const isSelected = selectedId === department.id;
	const isExpanded = expandedIds.includes(department.id);
	const hasChildren = department.hasChildren;

	const {
		departmentChildren,
		isLoading,
		isError,
		errorMessage,
		hasNextPage,
		isFetchingNextPage,
		isFetchNextPageError,
		fetchNextPage,
		refetch,
	} = useInfiniteDepartmentChildren({
		request: { parentId: department.id },
		enabled: isExpanded && hasChildren,
		stateId,
	});

	const handleToggle = () => {
		toggleDepartmentTreeExpandedId(department.id, hasChildren, stateId);
	};

	const handleSelect = () => {
		setDepartmentTreeSelectedId(department.id, stateId);
	};

	const handleLoadMore = () => {
		if (!hasNextPage || isFetchingNextPage) return;

		void fetchNextPage();
	};

	const handleRetry = () => {
		void refetch();
	};

	return {
		departmentChildren,
		isLoading,
		isError,
		errorMessage,
		isSelected,
		isExpanded,
		hasChildren,
		hasNextPage,
		isFetchingNextPage,
		isFetchNextPageError,
		handleToggle,
		handleSelect,
		handleLoadMore,
		handleRetry,
	};
}
