"use client";

import { Location } from "@/entities/locations/model/types";
import { useLocationsQuery } from "@/entities/locations/model/use-locations-query";
import { CreateLocationDialog } from "@/features/locations/ui/create-location-dialog";
import { UpdateLocationDialog } from "@/features/locations/ui/update-location-dialog";
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
	const [createOpen, setCreateOpen] = useState(false);
	const [updateOpen, setUpdateOpen] = useState(false);
	const [isDelete, setIsDelete] = useState(false);

	const [selectedLocation, setSelectedLocation] = useState<Location>(
		{} as Location,
	);

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

				<Button
					type="button"
					onClick={() => setCreateOpen(true)}
					className="ml-auto"
				>
					Создать локацию
				</Button>

				<CreateLocationDialog open={createOpen} setOpen={setCreateOpen} />
				{selectedLocation && (
					<UpdateLocationDialog
						key={selectedLocation.id}
						location={selectedLocation}
						open={selectedLocation !== undefined && updateOpen}
						setOpen={setUpdateOpen}
					/>
				)}
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
							<LocationCard
								key={location.id}
								location={location}
								onEdit={() => {
									setSelectedLocation(location);
									setUpdateOpen(true);
								}}
								onDelete={() => setIsDelete(true)}
							/>
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
