"use client";

import type { DepartmentId, DepartmentShortDto } from "@/entities/departments";
import type { LocationDto } from "@/entities/locations";
import { ArchiveViewSwitch } from "@/shared/components/archive-view-switch";
import { Spinner } from "@/shared/components/ui/spinner";
import { useArchiveView } from "@/shared/hooks/use-archive-view";
import { ListEmpty } from "@/shared/ui/list-empty";
import { ListError } from "@/shared/ui/list-error";
import { useState } from "react";
import {
	SelectDepartmentDialog,
	SelectedDepartment,
} from "@/features/select-department";
import { CreateLocationDialog } from "@/features/create-location";
import { UpdateLocationDialog } from "@/features/update-location";
import { LocationCard } from "./location-card";
import { useInfiniteLocationsList } from "../model/use-infinite-locations-list";
import {
	removeLocationSelectedDepartments,
	setLocationSelectedDepartments,
	useLocationSelectedDepartments,
} from "../model/location-list-store";
import { LocationFilters } from "./location-filters";

const LOCATIONS_DEPARTMENT_SELECT_STATE_ID = "locations-department-select";

export function InfiniteLocationsList() {
	const [createOpen, setCreateOpen] = useState(false);
	const [updateOpen, setUpdateOpen] = useState(false);

	const [selectOpen, setSelectOpen] = useState(false);

	const [selectedLocation, setSelectedLocation] = useState<LocationDto | null>(
		null,
	);

	const { view, setView } = useArchiveView();
	const {
		locations,
		isPending,
		isError,
		error,
		isFetchingNextPage,
		cursorRef,
		refetch,
	} = useInfiniteLocationsList({
		request: {
			isActive: view === "active",
		},
	});

	const handleRemove = (departmentId: DepartmentId) => {
		removeLocationSelectedDepartments(departmentId);
	};

	const handleSelectedDepartmentsChange = (
		departments: DepartmentShortDto[],
	) => {
		setLocationSelectedDepartments(departments);
	};

	const selectedDepartments = useLocationSelectedDepartments();

	return (
		<div className="space-y-4">
			<SelectDepartmentDialog
				stateId={LOCATIONS_DEPARTMENT_SELECT_STATE_ID}
				open={selectOpen}
				setOpen={setSelectOpen}
				selectedDepartments={selectedDepartments}
				onChange={handleSelectedDepartmentsChange}
				multiSelect
			/>

			<SelectedDepartment
				selectedDepartments={selectedDepartments}
				onRemove={handleRemove}
			/>

			<div className="flex flex-wrap items-center justify-between gap-3">
				<div>
					<h1 className="text-2xl font-bold tracking-tight">Локации</h1>
					<p className="text-muted-foreground text-sm">
						{view === "active" ? "Действующие локации" : "Удалённые локации"}
					</p>
				</div>

				<ArchiveViewSwitch
					value={view}
					onValueChange={setView}
					title="Режим отображения локаций"
				/>
			</div>

			<LocationFilters />

			<div className="space-y-2">
				{view === "active" && (
					<CreateLocationDialog open={createOpen} setOpen={setCreateOpen} />
				)}

				{view === "active" && selectedLocation && (
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
				<ListError
					message={error?.message ?? "Неизвестная ошибка"}
					onRetry={refetch}
				/>
			) : locations?.length === 0 ? (
				<ListEmpty title="Локация" />
			) : (
				<>
					<div className="grid min-w-0 gap-4 md:grid-cols-2 xl:grid-cols-4">
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

					<div ref={cursorRef} className="flex justify-center py-10">
						{isFetchingNextPage && <Spinner />}
					</div>
				</>
			)}
		</div>
	);
}
