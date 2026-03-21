"use client";

import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Separator } from "@/shared/components/ui/separator";
import { Spinner } from "@/shared/components/ui/spinner";
import {
	ChevronDown,
	ChevronUp,
	CircleCheckBig,
	CircleX,
	Clock3,
	Globe2,
	MapPin,
} from "lucide-react";
import { useEffect, useState } from "react";
import { locationsApi } from "../api";
import { Location } from "../types";

export const PAGE_SIZE = 10;

export function LocationsList() {
	const [isLoading, setIsLoading] = useState(true);
	const [page, setPage] = useState(1);
	const [locations, setLocations] = useState<Location[]>([]);
	const [error, setError] = useState<string | null>(null);
	const [expandedLocationId, setExpandedLocationId] = useState<string | null>(null);

	useEffect(() => {
		let isMounted = true;

		locationsApi
			.getLocations({ page, pageSize: PAGE_SIZE })
			.then((data) => {
				if (!isMounted) return;
				setLocations(data);
			})
			.catch((error: unknown) => {
				if (!isMounted) return;
				setError(error instanceof Error ? error.message : "Не удалось загрузить локации");
			})
			.finally(() => {
				if (!isMounted) return;
				setIsLoading(false);
			});

		return () => {
			isMounted = false;
		};
	}, [page]);

	if (isLoading) {
		return (
			<div className="flex min-h-60 items-center justify-center">
				<Spinner />
			</div>
		);
	}

	if (error) {
		return (
			<Card className="border-destructive/40">
				<CardContent className="py-10 text-center">
					<p className="text-sm font-medium text-destructive">Ошибка: {error}</p>
				</CardContent>
			</Card>
		);
	}

	if (!locations.length) {
		return (
			<Card className="border-dashed">
				<CardContent className="flex flex-col items-center justify-center py-10 text-center">
					<span className="text-lg font-semibold">Локации не найдены</span>
					<p className="mt-2 text-sm text-muted-foreground">
						Попробуй изменить параметры поиска.
					</p>
				</CardContent>
			</Card>
		);
	}

	return (
		<div className="space-y-4">
			<div>
				<h1 className="text-2xl font-bold tracking-tight">Локации</h1>
				<p className="text-sm text-muted-foreground">
					Список доступных локаций и их подробности.
				</p>
			</div>

			<div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
				{locations.map((location) => {
					const isExpanded = expandedLocationId === location.id;

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

								<Button
									type="button"
									variant="ghost"
									size="sm"
									className="w-fit gap-2 px-0 text-muted-foreground hover:text-foreground"
									onClick={() =>
										setExpandedLocationId(isExpanded ? null : location.id)
									}
								>
									{isExpanded ? (
										<>
											<ChevronUp className="h-4 w-4" />
											Скрыть подробности
										</>
									) : (
										<>
											<ChevronDown className="h-4 w-4" />
											Показать подробности
										</>
									)}
								</Button>
							</CardHeader>

							<CardContent className="space-y-3">
								<div className="flex items-center gap-2 text-sm text-muted-foreground">
									<Globe2 className="h-4 w-4 shrink-0" />
									<span>{location.timezone}</span>
								</div>

								{isExpanded && (
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
								)}
							</CardContent>
						</Card>
					);
				})}
			</div>
		</div>
	);
}