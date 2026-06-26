"use client";

import { DepartmentId } from "@/entities/departments/model/types";
import { Location } from "@/entities/locations/model/types";
import { useInfinityLocationsList } from "@/entities/locations/model/use-infinity-locations-list";
import {
	removeLocationSelectedDepartments as removeDepartmentSelectedLocations,
	setLocationSelectedDepartments,
	useLocationSelectedDepartments,
} from "@/features/locations/model/location-list-store";
import { CreateLocationDialog } from "@/features/locations/ui/create-location-dialog";
import { UpdateLocationDialog } from "@/features/locations/ui/update-location-dialog";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { SelectedDepartment } from "@/widgets/departments/ui/selected-department";
import { ListEmpty } from "@/widgets/list-empty";
import { ListError } from "@/widgets/list-error";
import { LocationCard } from "@/widgets/locations-list/ui/location-card";
import { useState } from "react";
import { SelectDepartmentDialog } from "../../departments/ui/select-department-dialog";
import { LocationFilters } from "./location-filters";

export function InfinityLocationsList() {
	const [createOpen, setCreateOpen] = useState(false);
	const [updateOpen, setUpdateOpen] = useState(false);
	const [, setIsDelete] = useState(false);

	const [selectOpen, setSelectOpen] = useState(false);

	const [selectedLocation, setSelectedLocation] = useState<Location | null>(
		null,
	);

	const selectedDepartments = useLocationSelectedDepartments();

	const {
		locations,
		isPending,
		isError,
		error,
		isFetchingNextPage,
		cursorRef,
	} = useInfinityLocationsList({});

	const handleRemove = (departmentId: DepartmentId) => {
		removeDepartmentSelectedLocations(departmentId);
	};

	return (
		<div className="space-y-4">
			<LocationFilters />

			<SelectDepartmentDialog
				open={selectOpen}
				setOpen={setSelectOpen}
				selectedDepartments={selectedDepartments}
				onChange={(selectedDepartments) =>
					setLocationSelectedDepartments(selectedDepartments)
				}
				multiSelect
			/>

			<SelectedDepartment
				selectedDepartments={selectedDepartments}
				onRemove={handleRemove}
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
