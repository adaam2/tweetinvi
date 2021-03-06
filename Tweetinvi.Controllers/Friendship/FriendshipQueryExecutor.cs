﻿using System.Collections.Generic;
using System.Linq;
using Tweetinvi.Core;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Core.Interfaces.Credentials;
using Tweetinvi.Core.Interfaces.DTO;
using Tweetinvi.Core.Interfaces.DTO.QueryDTO;
using Tweetinvi.Core.Interfaces.Models;
using Tweetinvi.Core.Interfaces.QueryGenerators;
using Tweetinvi.Core.Interfaces.QueryValidators;

namespace Tweetinvi.Controllers.Friendship
{
    public interface IFriendshipQueryExecutor
    {
        IEnumerable<long> GetUserIdsRequestingFriendship(int maximumUserIdsToRetrieve);
        IEnumerable<long> GetUserIdsYouRequestedToFollow(int maximumUserIdsToRetrieve);
        IEnumerable<long> GetUserIdsWhoseRetweetsAreMuted(); 

        // Create Friendship
        bool CreateFriendshipWith(IUserIdentifier userDTO);
        bool CreateFriendshipWith(long userId);
        bool CreateFriendshipWith(string userScreenName);

        // Destroy Friendship
        bool DestroyFriendshipWith(IUserIdentifier userDTO);
        bool DestroyFriendshipWith(long userId);
        bool DestroyFriendshipWith(string userScreenName);

        // Update Friendship Authorization
        bool UpdateRelationshipAuthorizationsWith(IUserIdentifier userIdentifier, IFriendshipAuthorizations friendshipAuthorizations);
        bool UpdateRelationshipAuthorizationsWith(long userId, IFriendshipAuthorizations friendshipAuthorizations);
        bool UpdateRelationshipAuthorizationsWith(string userScreenName, IFriendshipAuthorizations friendshipAuthorizations);

        // Get Existing Relationship
        IRelationshipDetailsDTO GetRelationshipBetween(IUserIdentifier sourceUserIdentifier, IUserIdentifier targetUserIdentifier);

        // Get Multiple Relationships
        IEnumerable<IRelationshipStateDTO> GetMultipleRelationshipsQuery(IEnumerable<IUserIdentifier> targetUserIdentifiers);
        IEnumerable<IRelationshipStateDTO> GetMultipleRelationshipsQuery(IEnumerable<long> targetUserIds);
        IEnumerable<IRelationshipStateDTO> GetMultipleRelationshipsQuery(IEnumerable<string> targetUsersScreenName);
    }

    public class FriendshipQueryExecutor : IFriendshipQueryExecutor
    {

        private readonly IFriendshipQueryGenerator _friendshipQueryGenerator;
        private readonly IUserQueryValidator _userQueryValidator;
        private readonly ITwitterAccessor _twitterAccessor;

        public FriendshipQueryExecutor(
            IFriendshipQueryGenerator friendshipQueryGenerator,
            IUserQueryValidator userQueryValidator,
            ITwitterAccessor twitterAccessor)
        {
            _twitterAccessor = twitterAccessor;
            _friendshipQueryGenerator = friendshipQueryGenerator;
            _userQueryValidator = userQueryValidator;
        }

        public IEnumerable<long> GetUserIdsRequestingFriendship(int maximumUserIdsToRetrieve)
        {
            string query = _friendshipQueryGenerator.GetUserIdsRequestingFriendshipQuery();
            return _twitterAccessor.ExecuteCursorGETQuery<long, IIdsCursorQueryResultDTO>(query, maximumUserIdsToRetrieve);
        }

        public IEnumerable<long> GetUserIdsYouRequestedToFollow(int maximumUserIdsToRetrieve)
        {
            string query = _friendshipQueryGenerator.GetUserIdsYouRequestedToFollowQuery();
            return _twitterAccessor.ExecuteCursorGETQuery<long, IIdsCursorQueryResultDTO>(query, maximumUserIdsToRetrieve);
        }

        public IEnumerable<long> GetUserIdsWhoseRetweetsAreMuted()
        {
            string query = _friendshipQueryGenerator.GetUserIdsWhoseRetweetsAreMutedQuery();
            return _twitterAccessor.ExecuteGETQuery<long[]>(query);
        }

        // Create Friendship
        public bool CreateFriendshipWith(IUserIdentifier userDTO)
        {
            string query = _friendshipQueryGenerator.GetCreateFriendshipWithQuery(userDTO);
            return _twitterAccessor.TryExecutePOSTQuery(query);
        }

        public bool CreateFriendshipWith(long userId)
        {
            string query = _friendshipQueryGenerator.GetCreateFriendshipWithQuery(userId);
            return _twitterAccessor.TryExecutePOSTQuery(query);
        }

        public bool CreateFriendshipWith(string userScreenName)
        {
            string query = _friendshipQueryGenerator.GetCreateFriendshipWithQuery(userScreenName);
            return _twitterAccessor.TryExecutePOSTQuery(query);
        }

        // Destroy Friendship
        public bool DestroyFriendshipWith(IUserIdentifier userDTO)
        {
            if (!_userQueryValidator.CanUserBeIdentified(userDTO))
            {
                return false;
            }

            string query = _friendshipQueryGenerator.GetDestroyFriendshipWithQuery(userDTO);
            return _twitterAccessor.TryExecutePOSTQuery(query);
        }

        public bool DestroyFriendshipWith(long userId)
        {
            string query = _friendshipQueryGenerator.GetDestroyFriendshipWithQuery(userId);
            return _twitterAccessor.TryExecutePOSTQuery(query);
        }

        public bool DestroyFriendshipWith(string userScreenName)
        {
            string query = _friendshipQueryGenerator.GetDestroyFriendshipWithQuery(userScreenName);
            return _twitterAccessor.TryExecutePOSTQuery(query);
        }

        // Update Friendship Authorizations
        public bool UpdateRelationshipAuthorizationsWith(IUserIdentifier userIdentifier, IFriendshipAuthorizations friendshipAuthorizations)
        {
            if (!_userQueryValidator.CanUserBeIdentified(userIdentifier))
            {
                return false;
            }

            string query = _friendshipQueryGenerator.GetUpdateRelationshipAuthorizationsWithQuery(userIdentifier, friendshipAuthorizations);
            return _twitterAccessor.TryExecutePOSTQuery(query);
        }

        public bool UpdateRelationshipAuthorizationsWith(long userId, IFriendshipAuthorizations friendshipAuthorizations)
        {
            string query = _friendshipQueryGenerator.GetUpdateRelationshipAuthorizationsWithQuery(userId, friendshipAuthorizations);
            return _twitterAccessor.TryExecutePOSTQuery(query);
        }

        public bool UpdateRelationshipAuthorizationsWith(string userScreenName, IFriendshipAuthorizations friendshipAuthorizations)
        {
            string query = _friendshipQueryGenerator.GetUpdateRelationshipAuthorizationsWithQuery(userScreenName, friendshipAuthorizations);
            return _twitterAccessor.TryExecutePOSTQuery(query);
        }

        // Relationship Between
        public IRelationshipDetailsDTO GetRelationshipBetween(IUserIdentifier sourceUserIdentifier, IUserIdentifier targetUserIdentifier)
        {
            var query = _friendshipQueryGenerator.GetRelationshipDetailsQuery(sourceUserIdentifier, targetUserIdentifier);
            return _twitterAccessor.ExecuteGETQuery<IRelationshipDetailsDTO>(query);
        }

        // Get Relationship with
        public IEnumerable<IRelationshipStateDTO> GetMultipleRelationshipsQuery(IEnumerable<IUserIdentifier> targetUserIdentifiers)
        {
            var targetUserIdentifiersArray = IEnumerableExtension.GetDistinctUserIdentifiers(targetUserIdentifiers);

            var distinctRelationships = new List<IRelationshipStateDTO>();

            for (int i = 0; i < targetUserIdentifiersArray.Length; i += TweetinviConsts.FRIENDSHIP_MAX_NUMBER_OF_FRIENDSHIP_TO_GET_IN_A_SINGLE_QUERY)
            {
                var userIdentifiersToAnalyze = targetUserIdentifiersArray.Skip(i).Take(TweetinviConsts.FRIENDSHIP_MAX_NUMBER_OF_FRIENDSHIP_TO_GET_IN_A_SINGLE_QUERY).ToArray();
                var query = _friendshipQueryGenerator.GetMultipleRelationshipsQuery(userIdentifiersToAnalyze);
                var relationshipStateDtos = _twitterAccessor.ExecuteGETQuery<IEnumerable<IRelationshipStateDTO>>(query);

                // As soon as we cannot retrieve additional objects, we stop the query
                if (relationshipStateDtos == null)
                {
                    break;
                }

                distinctRelationships.AddRange(relationshipStateDtos);
            }

            return distinctRelationships;
        }

        public IEnumerable<IRelationshipStateDTO> GetMultipleRelationshipsQuery(IEnumerable<long> targetUserIds)
        {
            var targetUserIdsArray = targetUserIds.Distinct().ToArray();
            var distinctRelationships = new List<IRelationshipStateDTO>();

            for (int i = 0; i < targetUserIdsArray.Length; i += TweetinviConsts.FRIENDSHIP_MAX_NUMBER_OF_FRIENDSHIP_TO_GET_IN_A_SINGLE_QUERY)
            {
                var userIdsToAnalyze = targetUserIdsArray.Skip(i).Take(TweetinviConsts.FRIENDSHIP_MAX_NUMBER_OF_FRIENDSHIP_TO_GET_IN_A_SINGLE_QUERY).ToArray();
                var query = _friendshipQueryGenerator.GetMultipleRelationshipsQuery(userIdsToAnalyze);
                var relationshipStateDtos = _twitterAccessor.ExecuteGETQuery<IEnumerable<IRelationshipStateDTO>>(query);

                // As soon as we cannot retrieve additional objects, we stop the query
                if (relationshipStateDtos == null)
                {
                    break;
                }

                distinctRelationships.AddRange(relationshipStateDtos);
            }

            return distinctRelationships;
        }

        public IEnumerable<IRelationshipStateDTO> GetMultipleRelationshipsQuery(IEnumerable<string> targetUsersScreenName)
        {
            var targetUserScreenNamesArray = targetUsersScreenName.Distinct().ToArray();
            var distinctRelationships = new List<IRelationshipStateDTO>();

            for (int i = 0; i < targetUserScreenNamesArray.Length; i += TweetinviConsts.FRIENDSHIP_MAX_NUMBER_OF_FRIENDSHIP_TO_GET_IN_A_SINGLE_QUERY)
            {
                var userScreenNamesToAnalyze = targetUserScreenNamesArray.Skip(i).Take(TweetinviConsts.FRIENDSHIP_MAX_NUMBER_OF_FRIENDSHIP_TO_GET_IN_A_SINGLE_QUERY).ToArray();
                var query = _friendshipQueryGenerator.GetMultipleRelationshipsQuery(userScreenNamesToAnalyze);
                var relationshipStateDtos = _twitterAccessor.ExecuteGETQuery<IEnumerable<IRelationshipStateDTO>>(query);

                // As soon as we cannot retrieve additional objects, we stop the query
                if (relationshipStateDtos == null)
                {
                    break;
                }

                distinctRelationships.AddRange(relationshipStateDtos);
            }

            return distinctRelationships;
        }
    }
}