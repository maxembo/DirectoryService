"use client";

import {
	DepartmentId,
	DepartmentShortDto,
} from "@/entities/departments/model/types";
import { Location } from "@/entities/locations/model/types";
import { SelectDepartmentDialog } from "@/features/departments/select-department/ui/select-department-dialog";
import { SelectedDepartment } from "@/features/departments/select-department/ui/selected-department";
import { CreateLocationDialog } from "@/features/locations/create-location/ui/create-location-dialog";
import {
	removeLocationSelectedDepartments,
	setLocationSelectedDepartments,
	useLocationSelectedDepartments,
} from "@/features/locations/model/location-list-store";
import { useInfiniteLocationsList } from "@/features/locations/model/use-infinite-locations-list";
import { LocationFilters } from "@/features/locations/ui/filters/location-filters";
import { UpdateLocationDialog } from "@/features/locations/update-location/ui/update-location-dialog";
import { Spinner } from "@/shared/components/ui/spinner";
import { ListEmpty } from "@/widgets/list-empty";
import { ListError } from "@/widgets/list-error";
import { LocationCard } from "@/widgets/locations-list/ui/location-card";
import { useState } from "react";

const LOCATIONS_DEPARTMENT_SELECT_STATE_ID = "locations-department-select";

export function InfiniteLocationsList() {
	const [createOpen, setCreateOpen] = useState(false);
	const [updateOpen, setUpdateOpen] = useState(false);
	const [, setIsDelete] = useState(false);

	const [selectOpen, setSelectOpen] = useState(false);

	const [selectedLocation, setSelectedLocation] = useState<Location | null>(
		null,
	);

	const {
		locations,
		isPending,
		isError,
		error,
		isFetchingNextPage,
		cursorRef,
	} = useInfiniteLocationsList({});

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
			<LocationFilters />

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
					<div className="grid min-w-0 gap-4 md:grid-cols-2 xl:grid-cols-4">
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
