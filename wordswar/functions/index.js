/* eslint-disable max-len */
const { onCall, HttpsError } = require("firebase-functions/v2/https");
const admin = require("firebase-admin");

// Initialize Firebase Admin SDK
admin.initializeApp();
const database = admin.database();

// Define the Gen 2 Cloud Function for getting a joker hint
exports.getJokerHint2 = onCall(
    {
        // Optional: Add runtime options like memory, timeout, etc.
        // Example: memory: '256MiB', timeoutSeconds: 120
    },
    async (request) => {
        try {
            console.log("Fetching joker hint...");

            // Ensure the user is authenticated
            if (!request.auth) {
                throw new HttpsError("unauthenticated", "You must be authenticated to use a joker hint.");
            }

            const gameId = request.data.gameId; // ID of the current game
            const selectedTopic = request.data.selectedTopic; // Selected topic for the game

            console.log("Game ID:", gameId);
            console.log("Selected Topic:", selectedTopic);

            // Fetch all used words for the current game from the database
            const usedWordsSnapshot = await database.ref(`/games/${gameId}/gameInfo/usedwords`).once("value");
            const usedWordsObject = usedWordsSnapshot.val() || {}; // If no used words, initialize as an empty object

            // Flatten used words from all players into a single array and convert to lowercase
            const usedWords = Object.values(usedWordsObject).flatMap((playerWords) =>
                Object.values(playerWords).flatMap((categoryWords) =>
                    Object.values(categoryWords).map((word) => word.toLowerCase()),
                ),
            );

            console.log("Used Words:", usedWords);

            // Fetch all words under the selected topic from the database
            const topicSnapshot = await database.ref(`/topics/${selectedTopic}`).once("value");
            const topicWords = topicSnapshot.val();

            if (!topicWords) {
                console.log(`Topic '${selectedTopic}' does not exist.`);
                throw new HttpsError("not-found", `Topic '${selectedTopic}' does not exist.`);
            }

            console.log("Topic Words:", topicWords);

            // Extract only primary words (keys of the topicWords object)
            const primaryWords = Object.keys(topicWords).map((word) => word.toLowerCase());

            console.log("Primary Words:", primaryWords);

            // Filter out primary words that have been used
            const availablePrimaryWords = primaryWords.filter((word) => !usedWords.includes(word));

            console.log("Available Primary Words:", availablePrimaryWords);

            // If no available primary words found after filtering, return an error
            if (availablePrimaryWords.length === 0) {
                console.log("No available primary words for joker hint.");
                throw new HttpsError("failed-precondition", "No available primary words for joker hint.");
            }

            // Check if the user has enough hints available
            const userId = request.auth.uid;
            const userHintDocRef = admin.firestore().collection("users").doc(userId).collection("hints").doc("hintsData");
            const userHintData = (await userHintDocRef.get()).data();
            const userHintCount = userHintData ? userHintData["joker"] || 0 : 0;

            if (userHintCount <= 0) {
                console.log("User doesn't have enough hints.");
                throw new HttpsError("failed-precondition", "You don't have enough hints.");
            }

            // Randomly select a primary word from the available primary words
            const randomPrimaryWord = availablePrimaryWords[Math.floor(Math.random() * availablePrimaryWords.length)];

            console.log("Selected Joker Hint Primary Word:", randomPrimaryWord);

            // Decrease the hint count for the user
            await userHintDocRef.update({
                ["joker"]: admin.firestore.FieldValue.increment(-1),
            });

            console.log("Hint count decreased.");

            return randomPrimaryWord; // Return just the joker hint primary word
        } catch (error) {
            console.error("Error fetching joker hint:", error);
            throw new HttpsError("internal", "An error occurred while fetching joker hint.", error);
        }
    },
);
