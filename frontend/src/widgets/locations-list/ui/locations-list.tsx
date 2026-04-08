"use client";

import { useLocationsQuery } from "@/entities/locations/model/use-locations-query";
import { LocationCreateDialog } from "@/features/create-location/ui/location-create-dialog";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { LocationCard } from "@/widgets/locations-list/ui/location-card";
import { LocationFiltersPanel } from "@/widgets/locations-list/ui/location-filters-panel";
import { LocationsListEmpty } from "@/widgets/locations-list/ui/location-list-empty";
import { LocationsListError } from "@/widgets/locations-list/ui/location-list-error";
import { LocationsPagination } from "@/widgets/locations-list/ui/locations-pagination";
import { useLocationFilters } from "@/widgets/model/use-location-filters";
import { useState } from "react";
import { useDebounce } from "use-debounce";

const DEBOUNCE_DELAY = 600;

export function LocationsList() {
	const [open, setOpen] = useState(false);
	const {
		filters,
		setPage,
		setSearch,
		setIsActive,
		setSortBy,
		setSortDirection,
	} = useLocationFilters();

	const [debouncedSearch] = useDebounce(filters.search, DEBOUNCE_DELAY);

	const { locations, totalPages, isPending, isError, error } =
		useLocationsQuery({
			search: debouncedSearch,
			isActive:
				filters.isActive === "all" ? undefined : filters.isActive === "active",
			sortBy: filters.sortBy,
			sortDirection: filters.sortDirection,
			departmentIds: filters.departmentIds,
			page: filters.page,
		});

	return (
		<div className="space-y-4">
			<LocationFiltersPanel
				search={filters.search}
				setSearch={setSearch}
				isActive={filters.isActive}
				setIsActive={setIsActive}
				sortBy={filters.sortBy}
				setSortBy={setSortBy}
				sortDirection={filters.sortDirection}
				setSortDirection={setSortDirection}
			/>

			<div className="space-y-2">
				<h1 className="text-2xl font-bold tracking-tight">Локации</h1>

				<Button type="button" onClick={() => setOpen(true)} className="ml-auto">
					Создать локацию
				</Button>

				<LocationCreateDialog open={open} setOpen={setOpen} />
			</div>

			{isPending ? (
				<div className="flex min-h-60 items-center justify-center">
					<Spinner />
				</div>
			) : isError ? (
				<LocationsListError message={error?.message ?? "Неизвестная ошибка"} />
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
						currentPage={filters.page}
						totalPages={totalPages}
						onPageChange={setPage}
					/>
				</>
			)}
		</div>
	);
}
