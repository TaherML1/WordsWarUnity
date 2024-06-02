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

exports.matchmaker = functions.database.ref("matchmaking/{playerId}")
    .onCreate((snap, context) => {
      const gameId = generateGameId();

      database.ref("matchmaking").once("value").then((players) => {
        let secondPlayer = null;
        players.forEach((player) => {
          if (player.val() === "placeholder" && player.key !== context.params.playerId) {
            secondPlayer = player;
          }
        });

        if (secondPlayer === null) return null;

        database.ref("matchmaking").transaction((matchmaking) => {
          // If any of the players gets into another game during the transaction, abort the operation
          if (matchmaking === null || matchmaking[context.params.playerId] !== "placeholder" || matchmaking[secondPlayer.key] !== "placeholder") return matchmaking;

          matchmaking[context.params.playerId] = gameId;
          matchmaking[secondPlayer.key] = gameId;
          return matchmaking;
        }).then((result) => {
          if (result.snapshot.child(context.params.playerId).val() !== gameId) return;

          const game = {
            gameInfo: {
              gameId: gameId,
              playersIds: [context.params.playerId, secondPlayer.key],
              scores: {
                [context.params.playerId]: 0, // Initialize player scores to 0
                [secondPlayer.key]: 0,
              },
              usedwords: {
                [context.params.playerId]: [""],
                [secondPlayer.key]: [""],
              },
              timer: 15, // Initialize timer to 15 seconds
            },
            turn: context.params.playerId,
          };

          database.ref("games/" + gameId).set(game).then((snapshot) => {
            console.log("Game created successfully!");
            return null;
          }).catch((error) => {
            console.log(error);
          });

          return null;
        }).catch((error) => {
          console.log(error);
        });

        return null;
      }).catch((error) => {
        console.log(error);
      });
    });

/**
* Generates a random game ID.
* @return {string} The randomly generated game ID.
*/
function generateGameId() {
  const possibleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  let gameId = "";
  for (let j = 0; j < 20; j++) gameId += possibleChars.charAt(Math.floor(Math.random() * possibleChars.length));
  return gameId;
}
