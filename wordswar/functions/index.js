/* eslint-disable require-jsdoc */
/* eslint-disable no-unused-vars */
/* eslint-disable max-len */
/**
 * Import function triggers from their respective submodules:
 *
 * const {onCall} = require("firebase-functions/v2/https");
 * const {onDocumentWritten} = require("firebase-functions/v2/firestore");
 *
 * See a full list of supported triggers at https://firebase.google.com/docs/functions
 */


// Create and deploy your first functions
// https://firebase.google.com/docs/functions/get-started

// index.js (assuming this is your Cloud Function file name)

const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();
const database = admin.database();


// Function to calculate the required XP for a given level
function calculateRequiredXP(level) {
  // Adjusted XP requirement based on level
  return ((level - 1) * 25) + 50; // for example : level 2   2* 25 +50 = 100 required xp
}


exports.LevelUpUser = functions.firestore.document("users/{userId}")
    .onUpdate(async (change, context) => {
      try {
        // Get the updated user data
        const userData = change.after.data();

        // Calculate the required XP for the next level
        const requiredXP = calculateRequiredXP(userData.level + 1);

        // Check if the player has enough XP to level up
        if (userData.xp >= requiredXP) {
          // Player has enough XP to level up
          const newLevel = userData.level + 1;

          // Deduct the required XP for the next level from the player's XP
          const remainingXP = userData.xp - requiredXP;

          // Update the user's level and XP in Firestore
          await change.after.ref.update({level: newLevel, xp: remainingXP});

          console.log(`User ${context.params.userId} leveled up to level ${newLevel}. Remaining XP: ${remainingXP}.`);
        } else {
          // Player does not have enough XP to level up
          console.log(`User ${context.params.userId} does not have enough XP to level up. Required XP for next level: ${requiredXP}.`);
        }
      } catch (error) {
        // Handle errors
        console.error("An error occurred while leveling up the user:", error);
      }
    });
