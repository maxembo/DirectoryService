"use client";

import { Location } from "@/entities/locations/model/types";
import { CreateLocationDialog } from "@/features/locations/create-location/ui/create-location-dialog";
import { useLocationFilters } from "@/features/locations/model/use-location-filters";
import { useLocationsList } from "@/features/locations/model/use-locations-list";
import { LocationFiltersPanel } from "@/features/locations/ui/filters/location-filters-panel";
import { UpdateLocationDialog } from "@/features/locations/update-location/ui/update-location-dialog";
import { PAGE_SIZE } from "@/shared/api/pagination-request";
import { Spinner } from "@/shared/components/ui/spinner";
import { ListEmpty } from "@/widgets/list-empty";
import { ListError } from "@/widgets/list-error";
import { LocationCard } from "@/widgets/locations-list/ui/location-card";
import { useState } from "react";
import { useDebounce } from "use-debounce";
import { LocationsPagination } from "./locations-pagination";

const DEBOUNCE_DELAY = 600;

export function LocationsList() {
	const [createOpen, setCreateOpen] = useState(false);
	const [updateOpen, setUpdateOpen] = useState(false);

	const [selectedLocation, setSelectedLocation] = useState<Location | null>(
		null,
	);

	const {
		filters,
		actions: { setSearch, setIsActive, setSortBy, setSortDirection, setPage },
	} = useLocationFilters();

	const [debouncedSearch] = useDebounce(filters.search, DEBOUNCE_DELAY);

	const { locations, totalPages, isPending, isError, error } = useLocationsList(
		{
			search: debouncedSearch,
			isActive:
				filters.isActive === "all" ? undefined : filters.isActive === "active",
			sortBy: filters.sortBy,
			sortDirection: filters.sortDirection,
			departmentIds: filters.departmentIds,
			page: filters.page,
			pageSize: PAGE_SIZE,
		},
	);

	return (
		<div className="space-y-4">
			<LocationFiltersPanel
				filters={filters}
				actions={{
					setSearch,
					setIsActive,
					setSortBy,
					setSortDirection,
				}}
			/>
			<div className="space-y-2">
				<h1 className="text-2xl font-bold tracking-tight">Локации</h1>

				<CreateLocationDialog open={createOpen} setOpen={setCreateOpen} />

				{selectedLocation && (
					<UpdateLocationDialog
						key={selectedLocation.id}
						location={selectedLocation}
						open={updateOpen}
						setOpen={setUpdateOpen}
					/>
				)}
			</div>

			{isPending ? (
				<div className="flex min-h-60 items-center justify-center">
					<Spinner />
				</div>
			) : isError ? (
				<ListError message={error?.message ?? "Неизвестная ошибка"} />
			) : locations?.length === 0 ? (
				<ListEmpty title="Локация" />
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
