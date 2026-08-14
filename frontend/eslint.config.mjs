import pluginQuery from "@tanstack/eslint-plugin-query";
import nextVitals from "eslint-config-next/core-web-vitals";
import nextTs from "eslint-config-next/typescript";
import { defineConfig, globalIgnores } from "eslint/config";

const eslintConfig = defineConfig([
	...nextVitals,
	...nextTs,
	...pluginQuery.configs["flat/recommended"],

	globalIgnores([".next/**", "out/**", "build/**", "next-env.d.ts"]),

	{
		files: ["src/**/*.{ts,tsx}"],
		rules: {
			"no-restricted-imports": [
				"error",
				{
					patterns: [
						{
							regex: "^@/(?:entities|features|widgets)/[^/]+/.+",
							message: "Используйте публичный API: @/<layer>/<slice>",
						},
						{
							regex: "^(?:\\.\\./)+(?:entities|features|widgets)/[^/]+/.+",
							message:
								"Используйте публичный API вместо относительного импорта другого slice",
						},
					],
				},
			],
		},
	},
]);

export default eslintConfig;
