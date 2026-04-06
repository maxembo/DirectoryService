"use client";

import { useLocationFilters } from "@/features/locations/hooks/use-location-filters";
import { useLocationQuery } from "@/features/locations/hooks/use-location-query";
import { LocationsPagination } from "@/features/locations/ui/locations-pagination";
import { Spinner } from "@/shared/components/ui/spinner";
import { useState } from "react";
import { useDebounce } from "use-debounce";
import { LocationCard } from "./location-card";
import { LocationCreateDialog } from "./location-create-dialog";
import { LocationFiltersPanel } from "./location-filters-panel";
import { LocationsListEmpty } from "./location-list-empty";
import { LocationsListError } from "./location-list-error";

const DEBOUNCE_DELAY = 600;

export function LocationsList() {
	const [open, setOpen] = useState<boolean>(false);
	const {
		state,
		setPage,
		setSearch,
		setIsActive,
		setSortBy,
		setSortDirection,
	} = useLocationFilters();

	const [debouncedSearch] = useDebounce(state.search, DEBOUNCE_DELAY);

	const { locations, totalPages, isPending, isError, error } = useLocationQuery(
		{
			search: debouncedSearch,
			isActive:
				state.isActive === "all" ? undefined : state.isActive === "active",
			sortBy: state.sortBy,
			sortDirection: state.sortDirection,
			departmentIds: state.departmentIds,
			page: state.page,
			pageSize: state.pageSize,
		},
	);

	return (
		<div className="space-y-4">
			<LocationFiltersPanel
				search={state.search}
				setSearch={setSearch}
				isActive={state.isActive}
				setIsActive={setIsActive}
				sortBy={state.sortBy}
				setSortBy={setSortBy}
				sortDirection={state.sortDirection}
				setSortDirection={setSortDirection}
			/>

			<div className="space-y-2">
				<h1 className="text-2xl font-bold tracking-tight">Локации</h1>
				<LocationCreateDialog open={open} setOpen={setOpen} />
			</div>

			{isPending ? (
				<div className="flex min-h-60 items-center justify-center">
					<Spinner />
				</div>
			) : isError ? (
				<LocationsListError
					message={error ? error.message : "Неизвестная ошибка"}
				/>
			) : locations?.length === 0 ? (
				<LocationsListEmpty />
			) : (
				<>
					<div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
						{locations.map((location) => (
							<LocationCard key={location.id} location={location} />
						))}
					</div>

					<LocationsPagination
						currentPage={state.page}
						totalPages={totalPages}
						onPageChange={setPage}
					/>
				</>
			)}
		</div>
	);
}
