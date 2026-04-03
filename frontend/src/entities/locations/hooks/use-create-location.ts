import { locationsApi } from "@/entities/locations/api";
import { useMutation } from "@tanstack/react-query";
import { queryClient } from "../../../shared/api/query-client";

export type CreateLocationParams = {
	name: string;
	address: {
		country: string;
		city: string;
		street: string;
		house: string;
	};
	timezone: string;
};

export function useCreateLocation(params: CreateLocationParams) {
	const {
		mutate: createLocation,
		isPending,
		error: createError,
	} = useMutation({
		mutationFn: () =>
			locationsApi.createLocation({
				name: params.name,
				address: {
					country: params.address.country,
					city: params.address.city,
					street: params.address.street,
					house: params.address.house,
				},
				timezone: params.timezone,
			}),
		onSettled: () => {
			queryClient.invalidateQueries({ queryKey: [locationsApi.baseKey] });
		},
	});

	return {
		createLocation,
		isPending,
		createError,
	};
}
