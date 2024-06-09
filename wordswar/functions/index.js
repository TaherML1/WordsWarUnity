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


exports.checkGameEnd = functions.database.ref("/games/{gameId}/gameEnd").onCreate(async (snapshot, context) => {
  const gameId = context.params.gameId;

  try {
    // Retrieve game information
    const gameSnapshot = await admin.database().ref(`/games/${gameId}`).once("value");
    const game = gameSnapshot.val();

    // Extract player IDs and scores
    const player1Id = game.gameInfo.playersIds[0];
    const player2Id = game.gameInfo.playersIds[1];
    const player1Score = game.gameInfo.scores[player1Id];
    const player2Score = game.gameInfo.scores[player2Id];

    // Determine winner and loser
    let winner = "";
    let loser = "";
    if (player1Score > player2Score) {
      winner = player1Id;
      loser = player2Id;
    } else if (player1Score < player2Score) {
      winner = player2Id;
      loser = player1Id;
    } else {
      // If both players have the same score, determine the loser based on the current turn
      const currentTurn = game.turn;
      if (currentTurn === player1Id) {
        // Player 1 is the loser if it's their turn
        winner = player2Id;
        loser = player1Id;
      } else {
        // Player 2 is the loser if it's their turn
        winner = player1Id;
        loser = player2Id;
      }
    }

    // Update game state
    await admin.database().ref(`/games/${gameId}`).update({
      winner: winner,
      loser: loser,
      status: "ended", // You can update other game status as needed
    });

    // Update coins for the winner and loser in Firestore
    const winnerRef = admin.firestore().collection("users").doc(winner);
    const loserRef = admin.firestore().collection("users").doc(loser);

    await admin.firestore().runTransaction(async (transaction) => {
      const winnerDoc = await transaction.get(winnerRef);
      const loserDoc = await transaction.get(loserRef);

      const winnerCoins = winnerDoc.data().coins + 20; // Add 20 coins for the winner
      const winnerScores = winnerDoc.data().scores + 100;
      const winnerXP = winnerDoc.data().xp + 25; // Add 20 XP for the winner
      const winnerMatchesWon = winnerDoc.data().matchesWon + 1;
      const loserCoins = loserDoc.data().coins + 10; // Add 10 coins for the loser
      const loserXP = loserDoc.data().xp + 15; // Add 10 XP for the loser
      const loserScore = loserDoc.data().scores + 50;
      const loserMatchLost = loserDoc.data().matchesLost + 1;

      transaction.update(winnerRef, {coins: winnerCoins, xp: winnerXP, scores: winnerScores, matchesWon: winnerMatchesWon});
      transaction.update(loserRef, {coins: loserCoins, xp: loserXP, scores: loserScore, matchesLost: loserMatchLost});
      console.log("winner Coins : ", winnerCoins);
      console.log("loser Coins : ", loserCoins);
    });

    console.log("Game ended:", gameId);
  } catch (error) {
    console.error("Error:", error);
  }
});
