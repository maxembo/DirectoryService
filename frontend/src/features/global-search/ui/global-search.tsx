import {
	Command,
	CommandDialog,
	CommandEmpty,
	CommandGroup,
	CommandInput,
	CommandItem,
	CommandList,
	CommandShortcut,
} from "@/shared/components/ui/command";
import { useGlobalSearch } from "../model/use-global-search";
import { useState } from "react";
import { Spinner } from "@/shared/components/ui/spinner";
import {
	type GlobalSearchDto,
	type GlobalSearchType,
} from "@/entities/global-search";
import { routes } from "@/shared/routes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Badge } from "@/shared/components/ui/badge";
import { SearchIcon } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
	Tooltip,
	TooltipContent,
	TooltipTrigger,
} from "@/shared/components/ui/tooltip";
import { ListError } from "@/shared/ui/list-error";
import { useSearchShortcut } from "../model/use-search-shortcut";

const groupLabels = {
	department: "Подразделение",
	location: "Локация",
	position: "Позиция",
} satisfies Record<GlobalSearchType, string>;

const searchRoutes = {
	department: routes.departments.href,
	location: routes.locations.href,
	position: routes.positions.href,
} satisfies Record<GlobalSearchType, string>;

function getSearchView(
	type: GlobalSearchType,
	isActive: boolean,
	deletedAt: string | null,
): "active" | "archived" {
	if (type === "department") {
		return deletedAt !== null ? "archived" : "active";
	}

	return isActive ? "active" : "archived";
}

export function GlobalSearch() {
	const [globalSearch, setGlobalSearch] = useState("");
	const [open, setOpen] = useState(false);

	useSearchShortcut(setOpen);

	const normalizedGlobalSearch = globalSearch.trim().toLowerCase();

	const {
		globalSearchList,
		isLoading,
		isSuccess,
		isError,
		error,
		isDebouncing,
	} = useGlobalSearch({ q: normalizedGlobalSearch });

	const router = useRouter();

	const handleOpenChange = (isOpen: boolean) => {
		if (!isOpen) {
			setGlobalSearch("");
		}

		setOpen(isOpen);
	};

	const handleSelect = (item: GlobalSearchDto) => {
		const params = new URLSearchParams({
			focus: item.id,
			view: getSearchView(item.type, item.isActive, item.deletedAt),
		});

		router.replace(`${searchRoutes[item.type]}?${params}`, {
			scroll: false,
		});

		handleOpenChange(false);
	};

	return (
		<div className="flex flex-col gap-4">
			<Tooltip>
				<TooltipTrigger asChild>
					<Button
						variant="ghost"
						size="icon"
						onClick={() => setOpen(true)}
						aria-label="Открыть поиск по справочнику"
					>
						<SearchIcon className="h-4 w-4" />
					</Button>
				</TooltipTrigger>

				<TooltipContent>
					<p>Поиск</p>
					<CommandShortcut className="ml-auto">⌘K</CommandShortcut>
				</TooltipContent>
			</Tooltip>
			<CommandDialog
				open={open}
				onOpenChange={handleOpenChange}
				className="sm:max-w-lg"
			>
				<Command shouldFilter={false}>
					<CommandInput
						value={globalSearch}
						onValueChange={setGlobalSearch}
						placeholder="Поиск по справочнику"
					/>
					<CommandList className="bg-popover text-popover-foreground mt-1 max-h-[min(30rem,calc(66dvh-10rem))]">
						{normalizedGlobalSearch.length >= 2 &&
							(isLoading || isDebouncing ? (
								<div className="flex justify-center py-4">
									<Spinner aria-label="Идёт поиск" />
								</div>
							) : (
								isError && (
									<ListError message={error?.message ?? "Неизвестная ошибка"} />
								)
							))}
						{normalizedGlobalSearch.length !== 0 &&
							normalizedGlobalSearch.length < 2 && (
								<div className="flex justify-center py-4">
									Введите минимум 2 символа
								</div>
							)}
						{normalizedGlobalSearch.length >= 2 &&
							isSuccess &&
							!isDebouncing && (
								<CommandEmpty className="">Ничего не найдено</CommandEmpty>
							)}

						{normalizedGlobalSearch.length >= 2 &&
							!isDebouncing &&
							Object.entries(groupLabels).map(([groupType, heading]) => {
								const items = globalSearchList.filter(
									(item) => item.type === groupType,
								);

								if (items.length === 0) return null;

								return (
									<CommandGroup key={groupType} heading={heading}>
										{items.map((item) => (
											<CommandItem
												className="gap-0 px-1 py-2.5"
												key={item.id}
												value={`${item.type}:${item.id}`}
												onSelect={() => handleSelect(item)}
											>
												<div className="min-w-0 flex-1">
													<div className="truncate text-sm font-medium">
														{item.title}
													</div>
													<div className="text-muted-foreground truncate text-xs">
														{item.subTitle}
													</div>
												</div>
												<Badge
													variant={item.isActive ? "default" : "secondary"}
												>
													{item.isActive ? "Активна" : "Неактивна"}
												</Badge>
											</CommandItem>
										))}
									</CommandGroup>
								);
							})}
					</CommandList>
				</Command>
			</CommandDialog>
		</div>
	);
}
