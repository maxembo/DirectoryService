"use client";

import { Location } from "@/entities/locations/model/types";
import { useInfinityLocationsList } from "@/entities/locations/model/use-infinity-locations-list";
import { useGetLocationFilters } from "@/features/locations/model/locations-filter-store";
import { CreateLocationDialog } from "@/features/locations/ui/create-location-dialog";
import { UpdateLocationDialog } from "@/features/locations/ui/update-location-dialog";
import { PAGE_SIZE } from "@/shared/api/pagination-request";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { LocationCard } from "@/widgets/locations-list/ui/location-card";
import { LocationsListEmpty } from "@/widgets/locations-list/ui/location-list-empty";
import { LocationsListError } from "@/widgets/locations-list/ui/location-list-error";
import { useState } from "react";
import { useDebounce } from "use-debounce";
import { LocationFilters } from "./location-filters";

const DEBOUNCE_DELAY = 600;

export function InfinityLocationsList() {
	const [createOpen, setCreateOpen] = useState(false);
	const [updateOpen, setUpdateOpen] = useState(false);
	const [, setIsDelete] = useState(false);

	const [selectedLocation, setSelectedLocation] = useState<Location | null>(null);

	const { search, isActive, sortBy, sortDirection, departmentIds } = useGetLocationFilters();

	const [debouncedSearch] = useDebounce(search, DEBOUNCE_DELAY);

	const { locations, isPending, isError, error, isFetchingNextPage, cursorRef } =
		useInfinityLocationsList({
			search: debouncedSearch,
			isActive:
				isActive === "all" ? undefined : isActive === "active",
			sortBy: sortBy,
			sortDirection: sortDirection,
			departmentIds: departmentIds,
			pageSize: PAGE_SIZE,
		});

	return (
		<div className="space-y-4">
			<LocationFilters />

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
				<LocationsListError message={error?.message ?? "Неизвестная ошибка"} />
			) : locations?.length === 0 ? (
				<LocationsListEmpty />
			) : (
				<>
					<div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
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


					<div ref={cursorRef} className="flex justify-center py-10">
						{isFetchingNextPage && <Spinner />}
					</div>
				</>
			)}
		</div>
	);
}
