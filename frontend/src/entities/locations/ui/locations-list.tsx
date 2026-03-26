"use client";

import { PaginationIconsOnly } from "@/features/pagination/pagination";
import { queryClient } from "@/shared/api/query-client";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import {
	Card,
	CardContent,
	CardHeader,
	CardTitle,
} from "@/shared/components/ui/card";
import { Input } from "@/shared/components/ui/input";
import { NativeSelect } from "@/shared/components/ui/native-select";
import { Separator } from "@/shared/components/ui/separator";
import { Spinner } from "@/shared/components/ui/spinner";
import { useDebounce } from "@/shared/debounce";
import { useMutation, useQuery } from "@tanstack/react-query";
import { CircleCheckBig, CircleX, Clock3, Globe2, MapPin } from "lucide-react";
import { useState } from "react";
import { locationsApi } from "../api";

export const PAGE_SIZE = 6;

export type ActiveFilter = "active" | "inactive" | "all";
export type SortByFilter = "name" | "created";
export type SortDirectionFilter = "asc" | "desc";

export function LocationsList() {
	const [page, setPage] = useState(1);
	const [search, setSearch] = useState("");
	const [departmentIds, setDepartmentIds] = useState<string[]>();
	const [isActive, setIsActive] = useState<ActiveFilter>("all");
	const [sortBy, setSortBy] = useState("");
	const [sortDirection, setSortDirection] = useState("");

	const debouncedSearch = useDebounce(search, 400);

	const { data, isLoading, isFetching, isError, error } = useQuery({
		queryFn: () =>
			locationsApi.getLocations({
				departmentIds: departmentIds,
				search: debouncedSearch,
				isActive: isActive === "all" ? undefined : isActive === "active",
				sortBy: sortBy,
				sortDirection: sortDirection,
				page: page,
				pageSize: PAGE_SIZE,
			}),
		queryKey: [
			"locations",
			{
				departmentIds,
				search: debouncedSearch,
				isActive,
				sortBy,
				sortDirection,
				page,
				pageSize: PAGE_SIZE,
			},
		],
	});

	const {
		mutate: createLocation,
		isPending,
		error: createError,
	} = useMutation({
		mutationFn: () =>
			locationsApi.createLocation({
				name: "Test Location frontend 10",
				address: {
					country: "frontend 10",
					city: "dkd",
					street: "dk",
					house: "4",
				},
				timezone: "Europe/Moscow",
			}),
		onSettled: () => {
			queryClient.invalidateQueries({ queryKey: ["locations"] });
		},
	});

	return (
		<div className="space-y-4">
			<Input
				placeholder="Поиск"
				value={search}
				onChange={(e) => {
					setPage(1);
					setSearch(e.target.value);
				}}
			/>
			<div className="flex items-center gap-4">
				<NativeSelect
					value={isActive}
					onChange={(e) => {
						const value = e.target.value as ActiveFilter;
						setIsActive(value);
						setPage(1);
					}}
				>
					<option value="all">Все</option>
					<option value="active">Активные</option>
					<option value="inactive">Неактивные</option>
				</NativeSelect>

				<NativeSelect
					value={sortBy}
					onChange={(e) => {
						const value = e.target.value as SortByFilter;
						setSortBy(value);
						setPage(1);
					}}
				>
					<option value="name">По имени</option>
					<option value="created">По дате создания</option>
				</NativeSelect>

				<NativeSelect
					value={sortDirection}
					onChange={(e) => {
						const value = e.target.value as SortDirectionFilter;
						setSortDirection(value);
						setPage(1);
					}}
				>
					<option value="asc">По возрастанию</option>
					<option value="desc">По убыванию</option>
				</NativeSelect>
			</div>

			<div className="space-y-2">
				<h1 className="text-2xl font-bold tracking-tight">Локации</h1>
				<Button onClick={() => createLocation()} disabled={isPending}>
					Создать локацию
				</Button>
				{createError && (
					<p className="text-sm text-destructive">
						Ошибка при создании: {createError.message}
					</p>
				)}
				{isFetching && (
					<p className="text-sm text-muted-foreground">Загрузка...</p>
				)}
			</div>

			{isLoading && !data ? (
				<div className="flex min-h-60 items-center justify-center">
					<Spinner />
				</div>
			) : isError ? (
				<Card className="border-destructive/40">
					<CardContent className="py-10 text-center">
						<p className="text-sm font-medium text-destructive">
							Ошибка: {error.message}
						</p>
					</CardContent>
				</Card>
			) : !data?.items?.length ? (
				<Card className="border-dashed">
					<CardContent className="flex flex-col items-center justify-center py-10 text-center">
						<span className="text-lg font-semibold">Локации не найдены</span>
						<p className="mt-2 text-sm text-muted-foreground">
							Попробуй изменить параметры поиска.
						</p>
					</CardContent>
				</Card>
			) : (
				<>
					<div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
						{data.items.map((location) => {
							return (
								<Card
									key={location.id}
									className="transition-shadow hover:shadow-md"
								>
									<CardHeader className="space-y-3">
										<div className="flex items-start justify-between gap-4">
											<CardTitle className="text-lg leading-tight">
												{location.name}
											</CardTitle>

											<Badge
												variant={location.isActive ? "default" : "secondary"}
												className="gap-1 whitespace-nowrap"
											>
												{location.isActive ? (
													<>
														<CircleCheckBig className="h-3.5 w-3.5" />
														Активна
													</>
												) : (
													<>
														<CircleX className="h-3.5 w-3.5" />
														Неактивна
													</>
												)}
											</Badge>
										</div>
									</CardHeader>

									<CardContent className="space-y-3">
										<div className="flex items-center gap-2 text-sm text-muted-foreground">
											<Globe2 className="h-4 w-4 shrink-0" />
											<span>{location.timezone}</span>
										</div>

										<div className="space-y-3 rounded-lg border bg-muted/30 p-3">
											<div className="flex items-start gap-2 text-sm">
												<MapPin className="mt-0.5 h-4 w-4 shrink-0 text-muted-foreground" />
												<div className="space-y-1">
													<p className="font-medium">Адрес</p>
													<ul className="space-y-1 text-muted-foreground">
														<li>Страна: {location.address.country}</li>
														<li>Город: {location.address.city}</li>
														<li>Улица: {location.address.street}</li>
														<li>Дом: {location.address.house}</li>
													</ul>
												</div>
											</div>

											<Separator />

											<div className="flex items-center gap-2 text-xs text-muted-foreground">
												<Clock3 className="h-3.5 w-3.5" />
												<span>ID: {location.id}</span>
											</div>
										</div>

										<div className="flex items-center gap-2 text-sm text-muted-foreground">
											<Clock3 className="h-4 w-4 shrink-0" />
											<span>Дата обновления: {location.updatedAt}</span>
										</div>
									</CardContent>
								</Card>
							);
						})}
					</div>
					<PaginationIconsOnly
						currentPage={page}
						totalPages={data.totalPages}
						onPageChange={setPage}
					/>
				</>
			)}
		</div>
	);
}
