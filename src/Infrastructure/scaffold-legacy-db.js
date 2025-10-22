#!/usr/bin/env node

const fs = require("fs");
const path = require("path");
const { execSync } = require("child_process");

/**
 * Database-First Scaffolding Script for Legacy Database
 * This script generates EF Core entities and DbContext from an existing database
 *
 * Usage:
 * 1. Add your legacy database connection string to appsettings.json:
 *    "ConnectionStrings": { "LegacyDatabase": "your-connection-string" }
 * 2. Run: npm run scaffold:legacy-db
 *
 * Cross-platform: Works on Windows, macOS, and Linux
 */

console.log("🔧 EF Core Database-First Scaffolding for Legacy Database");
console.log("==========================================================");

// Configuration
const OUTPUT_DIR = "./Entities/Legacy";
const CONTEXT_DIR = "./Persistence";
const NAMESPACE = "Infrastructure";
const APPSETTINGS_PATH = "../WebApi/appsettings.json";

// Helper function to run commands
function runCommand(command, description) {
    try {
        console.log(`📦 ${description}...`);
        execSync(command, { stdio: "inherit" });
        return true;
    } catch (error) {
        console.error(`❌ Failed to ${description.toLowerCase()}`);
        return false;
    }
}

// Helper function to create directory if it doesn't exist
function ensureDirectory(dir) {
    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
        console.log(`📁 Created directory: ${dir}`);
    }
}

// Main execution
try {
    // Check if appsettings.json exists
    if (!fs.existsSync(APPSETTINGS_PATH)) {
        console.error(`❌ appsettings.json not found at ${APPSETTINGS_PATH}`);
        process.exit(1);
    }

    // Read and parse appsettings.json
    let appSettings;
    try {
        const appSettingsContent = fs.readFileSync(APPSETTINGS_PATH, "utf8");
        appSettings = JSON.parse(appSettingsContent);
    } catch (error) {
        console.error("❌ Failed to parse appsettings.json");
        console.error(error.message);
        process.exit(1);
    }

    // Get connection string
    const connectionString = appSettings?.ConnectionStrings?.LegacyDatabase;

    if (!connectionString) {
        console.error("❌ LegacyDatabase connection string not found in appsettings.json");
        console.log("");
        console.log("Please add your legacy database connection string to appsettings.json:");
        console.log("{");
        console.log('  "ConnectionStrings": {');
        console.log(
            '    "LegacyDatabase": "Server=localhost;Database=LegacySystem;Trusted_Connection=true;TrustServerCertificate=true;"'
        );
        console.log("  }");
        console.log("}");
        process.exit(1);
    }

    console.log("✅ Using connection string from appsettings.json");

    // Create directories if they don't exist
    ensureDirectory(OUTPUT_DIR);

    // Install/Update EF Core Tools
    if (!runCommand("dotnet tool update --global dotnet-ef", "Installing/Updating EF Core Tools")) {
        process.exit(1);
    }

    console.log(`📋 Connection String: ${connectionString}`);
    console.log(`📁 Output Directory: ${OUTPUT_DIR}`);
    console.log(`🏗️  Context Directory: ${CONTEXT_DIR}`);
    console.log("");

    // Build the scaffold command
    const scaffoldCommand = [
        "dotnet ef dbcontext scaffold",
        `"${connectionString}"`,
        "Microsoft.EntityFrameworkCore.SqlServer",
        `--output-dir "${OUTPUT_DIR}"`,
        `--context-dir "${CONTEXT_DIR}"`,
        '--context "LegacyDbContext"',
        `--namespace "${NAMESPACE}.Entities.Legacy"`,
        `--context-namespace "${NAMESPACE}.Persistence"`,
        "--data-annotations",
        "--use-database-names",
        "--force",
    ].join(" ");

    // Run scaffolding
    console.log("🚀 Scaffolding database...");

    if (runCommand(scaffoldCommand, "Scaffolding database")) {
        console.log("✅ Scaffolding completed successfully!");
        console.log("");
        console.log("📝 Next steps:");
        console.log(`1. Review the generated entities in: ${OUTPUT_DIR}`);
        console.log(`2. Review the updated LegacyDbContext in: ${CONTEXT_DIR}`);
        console.log("3. Create repository interfaces in the Domain layer");
        console.log("4. Create repository implementations that inherit from LegacyRepository<T>");
        console.log("5. Register repositories in DependencyInjection.cs");
        console.log("");

        // List generated files
        console.log("🔍 Generated files:");
        try {
            const files = fs.readdirSync(OUTPUT_DIR).filter((file) => file.endsWith(".cs"));
            const displayFiles = files.slice(0, 10);
            displayFiles.forEach((file) => console.log(`   ${file}`));

            if (files.length > 10) {
                console.log(`   ... and ${files.length - 10} more files`);
            }
        } catch (error) {
            console.log("   (Could not list files)");
        }
    } else {
        console.log("❌ Scaffolding failed. Please check:");
        console.log("1. Database connection string is correct");
        console.log("2. Database server is running and accessible");
        console.log("3. You have necessary permissions");
        console.log("4. EF Core tools are properly installed");
        console.log("");
        console.log("💡 Connection string location:");
        console.log("   appsettings.json > ConnectionStrings > LegacyDatabase");
        process.exit(1);
    }
} catch (error) {
    console.error("❌ Unexpected error occurred:");
    console.error(error.message);
    process.exit(1);
}
