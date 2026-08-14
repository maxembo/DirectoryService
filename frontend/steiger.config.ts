import fsd from "@feature-sliced/steiger-plugin";
import { defineConfig } from "steiger";

export default defineConfig([
	{
		ignores: ["./src/shared/components/ui/**"],
	},

	...fsd.configs.recommended,

	{
		rules: {
			"fsd/insignificant-slice": "off",
		},
	},

	{
		files: ["./src/widgets/departments/ui/department-view.tsx"],
		rules: {
			// DepartmentView собирает активное и архивное представления.
			// DepartmentPositions пока оставляем отдельным виджетом.
			"fsd/forbidden-imports": "off",
		},
	},
]);
