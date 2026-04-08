import { locationsApi } from "@/entities/locations/api/locations-api";
import { EnvelopeError } from "@/shared/api/errors";
import { queryClient } from "@/shared/api/query-client";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateLocation() {
	const mutation = useMutation({
		mutationFn: locationsApi.updateLocation,
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: [locationsApi.baseKey] });
			toast.success("Локация успешно обновлена");
		},
		onError: (error) => {
			if (error instanceof EnvelopeError) {
				toast.error(error.allMessages);
				return;
			}
			toast.error("Ошибка при обновлении локации");
		},
	});

	return {
		updateLocation: mutation.mutateAsync,
		isPending: mutation.isPending,
		isError: mutation.isError,
		error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
		reset: mutation.reset,
	};
}
