import { locationsApi } from "@/entities/locations/api/api";
import { EnvelopeError } from "@/shared/api/errors";
import { queryClient } from "@/shared/api/query-client";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useRestoreLocation() {
	const mutation = useMutation({
		mutationFn: locationsApi.restoreLocation,
		onSuccess: async () => {
			await queryClient.invalidateQueries({ queryKey: [locationsApi.baseKey] });
			toast.success(`Локация успешно восстановлена`);
		},
		onError: (error) => {
			if (error instanceof EnvelopeError) {
				toast.error(error.message);
			} else {
				toast.error("Произошла ошибка при восстановлении локации");
			}
		},
	});

	return {
		restoreLocation: mutation.mutateAsync,
		isPending: mutation.isPending,
		isError: mutation.isError,
		error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
	};
}
