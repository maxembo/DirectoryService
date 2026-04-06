import { Button } from "@/shared/components/ui/button";
import {
	Dialog,
	DialogClose,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
	DialogTrigger,
} from "@/shared/components/ui/dialog";
import { Field, FieldGroup } from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { useState } from "react";
import { useCreateLocation } from "../hooks/use-create-location";

type Props = {
	open: boolean;
	setOpen: (open: boolean) => void;
};

type CreateFormData = {
	name: string;
	country: string;
	city: string;
	street: string;
	house: string;
	timezone: string;
};

export function LocationCreateDialog({ open, setOpen: setOpenChange }: Props) {
	const initialData: CreateFormData = {
		name: "",
		country: "",
		city: "",
		street: "",
		house: "",
		timezone: "",
	};
	const { createLocation, isPending, isError, error } = useCreateLocation();

	const [form, setForm] = useState(initialData);

	const handleChange = (field: string, value: string) => {
		setForm((prev) => ({ ...prev, [field]: value }));
	};

	const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		createLocation(
			{
				name: form.name,
				address: {
					country: form.country,
					city: form.city,
					street: form.street,
					house: form.house,
				},
				timezone: form.timezone,
			},
			{
				onSuccess: () => {
					setForm(initialData);
					setOpenChange(false);
				},
			},
		);
	};

	const getErrorMessage = (): string => {
		if (isError) {
			return error ? error.allMessages : "Неизвестная ошибка";
		}

		return "";
	};

	return (
		<Dialog open={open} onOpenChange={setOpenChange}>
			<DialogTrigger asChild>
				<Button type="button">Создать локацию</Button>
			</DialogTrigger>

			<DialogContent className="sm:max-w-lg">
				<form onSubmit={handleSubmit} className="space-y-6">
					<DialogHeader>
						<DialogTitle>Создание локации</DialogTitle>
						<DialogDescription>
							Заполните данные локации и нажмите сохранить.
						</DialogDescription>
					</DialogHeader>
					{error && (
						<p className="text-sm font-medium text-destructive">
							Ошибка: {getErrorMessage()}
						</p>
					)}

					<FieldGroup className="space-y-4">
						<Field>
							<Label htmlFor="name">Название</Label>
							<Input
								id="name"
								value={form.name}
								onChange={(e) => handleChange("name", e.target.value)}
							/>
						</Field>

						<div className="space-y-3 rounded-xl border bg-muted/20 p-4">
							<p className="text-sm font-medium">Адрес</p>

							<div className="grid gap-4 sm:grid-cols-2">
								<Field>
									<Label htmlFor="country">Страна</Label>
									<Input
										id="country"
										value={form.country}
										onChange={(e) => handleChange("country", e.target.value)}
									/>
								</Field>

								<Field>
									<Label htmlFor="city">Город</Label>
									<Input
										id="city"
										value={form.city}
										onChange={(e) => handleChange("city", e.target.value)}
									/>
								</Field>

								<Field className="sm:col-span-2">
									<Label htmlFor="street">Улица</Label>
									<Input
										id="street"
										value={form.street}
										onChange={(e) => handleChange("street", e.target.value)}
									/>
								</Field>

								<Field>
									<Label htmlFor="house">Дом</Label>
									<Input
										id="house"
										value={form.house}
										onChange={(e) => handleChange("house", e.target.value)}
									/>
								</Field>

								<Field>
									<Label htmlFor="timezone">Часовой пояс</Label>
									<Input
										id="timezone"
										value={form.timezone}
										onChange={(e) => handleChange("timezone", e.target.value)}
									/>
								</Field>
							</div>
						</div>
					</FieldGroup>

					<DialogFooter>
						<DialogClose asChild>
							<Button type="button" variant="outline">
								Отмена
							</Button>
						</DialogClose>
						<Button type="submit" disabled={isPending}>
							{isPending ? "Сохранение..." : "Сохранить"}
						</Button>
					</DialogFooter>
				</form>
			</DialogContent>
		</Dialog>
	);
}
