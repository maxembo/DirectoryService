"use client";

import { useCreateLocation } from "@/entities/locations/hooks/use-create-location";
import { useLocationList } from "@/entities/locations/hooks/use-location-list";
import { Pagination } from "@/features/pagination/pagination";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { useDebounce } from "@/shared/hooks/use-debounce";
import { useLocationFilters } from "../hooks/use-location-filters";
import { LocationCard } from "./location-card";
import { LocationFiltersPanel } from "./location-filters-panel";
import { LocationsListEmpty } from "./location-list-empty";
import { LocationsListError } from "./location-list-error";

export function LocationsList() {
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

	const {
		createLocation,
		isPending: isCreatePenging,
		createError,
	} = useCreateLocation({
		name: "Test Location frontend 22",
		address: { country: "frontend 22", city: "dkd", street: "dk", house: "4" },
		timezone: "UTC",
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
				<Button onClick={() => createLocation()} disabled={isCreatePenging}>
					Создать локацию
				</Button>
				{createError && (
					<p className="text-sm text-destructive">
						Ошибка при создании: {createError.message}
					</p>
				)}
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

					<Pagination
						currentPage={state.page}
						totalPages={totalPages}
						onPageChange={setPage}
					/>
				</>
			)}
		</div>
	);
}
