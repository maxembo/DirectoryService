import { Spinner } from "@/shared/components/ui/spinner";
import { ListEmpty } from "@/shared/ui/list-empty";
import { ListError } from "@/shared/ui/list-error";
import { useState } from "react";
import { useDebounce } from "use-debounce";
import { DepartmentFilters } from "@/features/department-list";
import { useInfiniteDepartmentsList } from "@/entities/departments";
import { ArchivedDepartmentCard } from "./archived-department-card";

export function ArchivedDepartmentList() {
	const [search, setSearch] = useState("");

	const [debouncedSearch] = useDebounce(search, 600);
	const {
		departments,
		isPending,
		isError,
		error,
		cursorRef,
		isFetchingNextPage,
		refetch,
	} = useInfiniteDepartmentsList({
		request: {
			isActive: false,
			isArchived: true,
			search: debouncedSearch,
		},
	});

	return (
		<div className="flex h-full min-h-0 flex-col gap-6">
			<DepartmentFilters search={search} setSearch={setSearch} />
			{isPending ? (
				<div className="flex min-h-60 items-center justify-center">
					<Spinner />
				</div>
			) : isError ? (
				<ListError
					message={error?.message ?? "Ошибка загрузки удалённых отделов"}
					onRetry={refetch}
				/>
			) : departments?.length === 0 ? (
				<ListEmpty title="Удалённые отделы" />
			) : (
				<>
					<div className="grid min-w-0 gap-4 md:grid-cols-2 xl:grid-cols-4">
						{departments?.map((department) => (
							<ArchivedDepartmentCard
								key={department.id}
								department={department}
							/>
						))}
					</div>
				</>
			)}

			<div ref={cursorRef} className="flex justify-center py-6">
				{isFetchingNextPage && <Spinner />}
			</div>
		</div>
	);
}
