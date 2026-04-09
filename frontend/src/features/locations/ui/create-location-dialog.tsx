import { Button } from "@/shared/components/ui/button";
import {
	Dialog,
	DialogClose,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/shared/components/ui/dialog";
import { Field, FieldGroup } from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { CreateLocationFormData, locationSchema } from "../model/location-form";
import { useCreateLocation } from "../model/use-create-location";

type Props = {
	open: boolean;
	setOpen: (open: boolean) => void;
};

const initialData: CreateLocationFormData = {
	name: "",
	address: {
		country: "",
		city: "",
		street: "",
		house: "",
	},
	timezone: "",
};

export function CreateLocationDialog({ open, setOpen }: Props) {
	const {
		register,
		handleSubmit,
		formState: { errors },
		reset,
	} = useForm<CreateLocationFormData>({
		defaultValues: initialData,
		resolver: zodResolver(locationSchema),
	});

	const {
		createLocation,
		isPending,
		isError,
		error,
		reset: resetCreateLocation,
	} = useCreateLocation();

	const handleClose = () => {
		reset();
		resetCreateLocation();
		setOpen(false);
	};

	const onSubmit = async (data: CreateLocationFormData) => {
		try {
			await createLocation(data);
			handleClose();
		} catch {}
	};

	const handleOpenChange = (nextOpen: boolean) => {
		if (!nextOpen) {
			handleClose();
			return;
		}

		setOpen(true);
	};

	return (
		<Dialog open={open} onOpenChange={handleOpenChange}>
			<DialogContent className="sm:max-w-md">
				<form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
					<DialogHeader>
						<DialogTitle>Создание локации</DialogTitle>
						<DialogDescription>
							Заполните данные локации и нажмите сохранить.
						</DialogDescription>
					</DialogHeader>
					{isError && (
						<p className="text-sm font-medium text-destructive">
							Ошибка: {error?.allMessages ?? "Неизвестная ошибка"}
						</p>
					)}

					<FieldGroup className="space-y-4">
						<Field>
							<Label htmlFor="name">Название</Label>
							<Input
								id="name"
								placeholder="Москва-сити"
								{...register("name")}
							/>
							{errors.name && (
								<p className="text-sm font-medium text-destructive">
									{errors.name.message}
								</p>
							)}
						</Field>

						<div className="space-y-4 rounded-xl border bg-muted/20 p-4">
							<p className="text-sm font-medium">Адрес</p>

							<div className="grid gap-3 sm:grid-cols-2">
								<Field>
									<Label htmlFor="country">Страна</Label>
									<Input
										id="country"
										placeholder="Россия"
										{...register("address.country")}
									/>
									{errors.address?.country && (
										<p className="text-sm font-medium text-destructive">
											{errors.address.country.message}
										</p>
									)}
								</Field>

								<Field>
									<Label htmlFor="city">Город</Label>
									<Input
										id="city"
										placeholder="Москва"
										{...register("address.city")}
									/>
									{errors.address?.city && (
										<p className="text-sm font-medium text-destructive">
											{errors.address.city.message}
										</p>
									)}
								</Field>
							</div>

							<div className="grid gap-3 sm:grid-cols-[minmax(0,2fr)_120px]">
								<Field>
									<Label htmlFor="street">Улица</Label>
									<Input
										id="street"
										placeholder="Пресненская набережная"
										{...register("address.street")}
									/>
									{errors.address?.street && (
										<p className="text-sm font-medium text-destructive">
											{errors.address.street.message}
										</p>
									)}
								</Field>

								<Field>
									<Label htmlFor="house">Дом</Label>
									<Input
										id="house"
										placeholder="15A"
										{...register("address.house")}
									/>
									{errors.address?.house && (
										<p className="text-sm font-medium text-destructive">
											{errors.address.house.message}
										</p>
									)}
								</Field>
							</div>
						</div>

						<Field>
							<Label htmlFor="timezone">Часовой пояс</Label>
							<Input
								id="timezone"
								placeholder="Europe/Moscow"
								{...register("timezone")}
							/>
							{errors.timezone && (
								<p className="text-sm font-medium text-destructive">
									{errors.timezone.message}
								</p>
							)}
						</Field>
					</FieldGroup>

					<DialogFooter className="pt-2">
						<DialogClose asChild>
							<Button type="button" variant="outline" onClick={handleClose}>
								Отмена
							</Button>
						</DialogClose>
						<Button type="submit" disabled={isPending}>
							{isPending ? "Создание..." : "Создать"}
						</Button>
					</DialogFooter>
				</form>
			</DialogContent>
		</Dialog>
	);
}
