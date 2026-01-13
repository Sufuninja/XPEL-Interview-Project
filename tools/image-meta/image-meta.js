const sharp = require('sharp');
const fs = require('fs');

async function extractMetadata(imagePath) {
    try {
        // Check if file exists
        if (!fs.existsSync(imagePath)) {
            throw new Error(`File not found: ${imagePath}`);
        }

        // Extract metadata using sharp
        const metadata = await sharp(imagePath).metadata();
        
        // Output the metadata as JSON
        console.log(JSON.stringify({
            Width: metadata.width,
            Height: metadata.height,
            Density: metadata.density || null,
            Format: metadata.format
        }));
    } catch (error) {
        console.error(error.message);
        process.exit(1);
    }
}

// Get the image path from command line arguments
const imagePath = process.argv[2];

if (!imagePath) {
    console.error('Usage: node image-meta.js <image-path>');
    process.exit(1);
}

extractMetadata(imagePath);