import { departmentsApi } from "@/entities/departments";
import { locationsApi } from "@/entities/locations";
import { positionsApi } from "@/entities/positions";
import { EnvelopeError } from "@/shared/api/errors";
import { queryClient } from "@/shared/api/query-client";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

type Options = {
	onRestored?: () => void;
};

export function useRestoreDepartment(options: Options) {
	const mutation = useMutation({
		mutationFn: departmentsApi.restoreDepartment,
		onSuccess: async () => {
			await Promise.all([
				queryClient.invalidateQueries({
					queryKey: [departmentsApi.baseKey],
				}),
				queryClient.invalidateQueries({
					queryKey: [locationsApi.baseKey],
				}),
				queryClient.invalidateQueries({
					queryKey: [positionsApi.baseKey],
				}),
			]);

			options.onRestored?.();

			toast.success(`Подразделение успешно восстановлено`);
		},
		onError: (error) => {
			if (error instanceof EnvelopeError) {
				toast.error(error.message);
			} else {
				toast.error("Произошла ошибка при восстановлении подразделения");
			}
		},
	});

	return {
		restoreDepartment: mutation.mutateAsync,
		isPending: mutation.isPending,
		isError: mutation.isError,
		error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
	};
}
