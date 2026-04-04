"use client";

import { useLocationList } from "@/entities/locations/models/use-location-list";
import { LocationsPagination } from "@/features/locations/locations-pagination";
import { Spinner } from "@/shared/components/ui/spinner";
import { useDebounce } from "@/shared/hooks/use-debounce";
import { useState } from "react";
import { LocationCard } from "../../../features/locations/location-card";
import { LocationCreateDialog } from "../../../features/locations/location-create-dialog";
import { LocationFiltersPanel } from "../../../features/locations/location-filters-panel";
import { LocationsListEmpty } from "../../../features/locations/location-list-empty";
import { LocationsListError } from "../../../features/locations/location-list-error";
import { useLocationFilters } from "../models/use-location-filters";

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

	const debouncedSearch = useDebounce(state.search, 400);

	const { locations, totalPages, isPending, error } = useLocationList({
		page: state.page,
		search: debouncedSearch,
		isActive:
			state.isActive === "all" ? undefined : state.isActive === "active",
		sortBy: state.sortBy,
		sortDirection: state.sortDirection,
		pageSize: state.pageSize,
		departmentIds: state.departmentIds,
	});

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
			) : error ? (
				<LocationsListError message={error.message} />
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
