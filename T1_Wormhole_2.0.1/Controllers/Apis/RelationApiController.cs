using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;

namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    [Route("api/Relation/[Action]")]
    [ApiController]
    public class RelationApiController : ControllerBase
    {
        private readonly WormHoleContext _db;

        public RelationApiController(WormHoleContext db)
        {
            _db = db;
        }
        [HttpGet]
        public async Task<IQueryable<RelationDto>> GetRelation()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin")
            {
                return null;
            }
            else if (role == "User")
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier);
                var id = Convert.ToInt32(currentUserId.Value);
                var relations = _db.Relations
                    .Where(r => r.InviterId == id && r.RelationType != "Block" || r.InviteeId == id && r.RelationType != "Block")
                    .Include(r => r.Inviter).Include(r => r.Invitee) // Join 資料
                    .Select(r => new RelationDto
                    {
                        RelationType = r.RelationType,
                        InviterId = r.InviterId,
                        InviterName = r.Inviter.Nickname,
                        InviteeId = r.InviteeId,
                        InviteeName = r.Invitee.Nickname,
                        Invite = r.Invite,
                    });
                return relations;
            }
            else
            {
                return null;
            }
        }
        [HttpPut]
        public async Task<string> AddRelation(RelationDto relationDto)
        {
            //var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier);              
            //var id = Convert.ToInt32(currentUserId.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin")
            {
                return "請管理員自重";
            }
            else if (role == "User")
            {
                Relation relation = new Relation
                {
                    InviterId = relationDto.InviterId,
                    InviteeId = relationDto.InviteeId,
                    RelationType = "Friend",
                    Invite = "申請中",
                };
                _db.Relations.Add(relation);
                _db.SaveChanges();
                return "已成功送出邀請";
            }
            else
            {
                return "請先登入或註冊";
            }

            //_db.Relations.Add();
            //_db.SaveChanges();
        }

        [HttpPut]
        public async Task<string> AcceptRelation(RelationDto relationDto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin")
            {
                return "請管理員自重";
            }
            else if (role == "User")
            {
                var relation = await _db.Relations
                    .FirstOrDefaultAsync(r => r.InviterId == relationDto.InviterId && r.InviteeId == relationDto.InviteeId);
                if (relation != null)
                {
                    relation.Invite = "已接受";
                    _db.SaveChanges();
                    return "已接受邀請";
                }
                else
                {
                    return "無此邀請";
                }
            }
            else
            {
                return "請先登入或註冊";
            }
        }

        [HttpPut]
        public async Task<string> RefuseRelation(RelationDto relationDto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin")
            {
                return "請管理員自重";
            }
            else if (role == "User")
            {
                var relation = await _db.Relations
                    .FirstOrDefaultAsync(r => r.InviterId == relationDto.InviterId && r.InviteeId == relationDto.InviteeId);
                if (relation != null)
                {
                    _db.Relations.Remove(relation);
                    _db.SaveChanges();
                    return "已拒絕邀請";
                }
                else
                {
                    return "無此邀請";
                }
            }
            else
            {
                return "請先登入或註冊";
            }
        }

        [HttpDelete]
        public async Task<string> DeleteRelation(RelationDto relationDto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin")
            {
                return "請管理員自重";
            }
            else if (role == "User")
            {
                var relation = await _db.Relations
                    .FirstOrDefaultAsync(
                    r => r.InviterId == relationDto.InviterId && r.InviteeId == relationDto.InviteeId ||
                    r.InviteeId == relationDto.InviterId && r.InviterId == relationDto.InviteeId);
                if (relation != null)
                {
                    _db.Relations.Remove(relation);
                    _db.SaveChanges();
                    return "已刪除邀請";
                }
                else
                {
                    return "無此邀請";
                }
            }
            else
            {
                return "請先登入或註冊";
            }
        }
        [HttpPut]
        public async Task<string> AddBlock(RelationDto relationDto)
        {
            //var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier);              
            //var id = Convert.ToInt32(currentUserId.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin")
            {
                return "請管理員自重";
            }
            else if (role == "User")
            {
                Relation relation = new Relation
                {
                    InviterId = relationDto.InviterId,
                    InviteeId = relationDto.InviteeId,
                    RelationType = "Block",
                    Invite = "已阻擋",
                };
                _db.Relations.Add(relation);
                _db.SaveChanges();
                return "已成功加入黑名單";
            }
            else
            {
                return "請先登入或註冊";
            }
        }
    }
}
