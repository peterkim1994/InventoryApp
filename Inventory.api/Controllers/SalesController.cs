﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Inventory.api.Controllers;
using InventoryPOS.Core.Dtos;
using InventoryPOS.DataStore.Models;
using InventoryPOSApp.Core.Dtos;
using InventoryPOSApp.Core.Repositories;
using InventoryPOSApp.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InventoryPOS.api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ILogger<SalesController> _logger;
        private ISalesService _service { get; set; }
        private ISalesRepository _repo { get; set; }
        private readonly IMapper _mapper;
        private readonly IInventoryService _inventoryService;

        public SalesController
        (
            ILogger<SalesController> logger,
            ISalesRepository repo,
            ISalesService service,
            IInventoryService inventoryService,
            IMapper mapper
        )
        {
            _logger = logger;
            _service = service;
            _repo = repo;
            _mapper = mapper;
            _inventoryService = inventoryService;
        }

        [HttpPost("AddPromotion")]
        public IActionResult AddPromotion(PromotionDto promotionDto)
        {
            if (ModelState.IsValid)
            {
                var promotion = _mapper.Map<PromotionDto, Promotion>(promotionDto);
                if (_service.AddPromotion(promotion))
                {
                    return Ok(promotionDto);
                }
                return BadRequest("that promtion already exists");
            }
            return BadRequest("Invalid promotion settings");
        }


        [HttpPost("AddProductPromotion")]
        public IActionResult AddProductPromotion(int productId, int promotionId)
        {
            if (_service.AddProductToPromotion(productId, promotionId))
            {
                return Ok(new ProductPromotion { ProductId = productId, PromotionId = promotionId });
            }
            return BadRequest("This promotion already contains this product");
        }


        [HttpGet("GetActivePromotions")]
        public IActionResult GetActivePromotions()
        {
            var promotions = _service.GetActivePromotions();
            var PromotionDtos = _mapper.Map<IList<Promotion>, IList<PromotionDto>>(promotions);
            return Ok(PromotionDtos);
        }


        [HttpGet("GetPromotionsProducts/{promotionId}")]
        public IActionResult GetPromotionsProducts(int promotionId)
        {
            var products = _repo.GetPromotionsProducts(promotionId);
            if (products == null)
                return BadRequest();
            var ProductDtos = _mapper.Map<IList<Product>, IList<ProductDto>>(products);
            return Ok(ProductDtos);
        }

        [HttpPost("AddProductPromotions")]
        public IActionResult AddProductPromotions(Object jsonResult)
        {
            dynamic reqBody = JObject.Parse(jsonResult.ToString());

            int promotionId = reqBody.promotionId;
            IList<int> productIds = reqBody.productIds.ToObject<IList<int>>();

            Promotion promo = _repo.GetPromotion(promotionId);
            if (promo == null)
            {
                return BadRequest("promotion doesnt exist");
            }
            foreach (var productId in productIds)
            {
                _service.AddProductToPromotion(productId, promotionId);
            }
            var promoDto = _mapper.Map<Promotion, PromotionDto>(promo);
            return Ok(promoDto);
        }

        [HttpDelete("DeletePromotion")]
        public IActionResult DeletePromotion(Object jsonResult)
        {
            dynamic reqBody = JObject.Parse(jsonResult.ToString());
            int promotionId = reqBody.promotionId;

            _service.DeletePromotion(promotionId);
            return Ok();
        }

        [HttpDelete("RemoveProductPromotions")]
        public IActionResult RemoveProductPromotions(Object jsonResult)
        {
            dynamic reqBody = JObject.Parse(jsonResult.ToString());

            int promotionId = reqBody.promotionId;
            IList<int> productIds = reqBody.productIds.ToObject<IList<int>>();

            Promotion promo = _repo.GetPromotion(promotionId);
            if (promo == null)
            {
                return BadRequest("promotion doesnt exist");
            }
            foreach (var productId in productIds)
            {
                _service.RemoveProductPromotion(productId, promotionId);
            }
            var promoDto = _mapper.Map<Promotion, PromotionDto>(promo);
            return Ok(promoDto);
        }

        [HttpPut("EditPromotion")]
        public IActionResult EditPromotion(PromotionDto promotionDto)
        {
            Promotion promo = _mapper.Map<PromotionDto, Promotion>(promotionDto);
            if (ModelState.IsValid)
            {
                _service.EditPromotion(promo);
                var promoDto = _mapper.Map<Promotion, PromotionDto>(promo);
                return Ok(promoDto);
            }
            if(String.IsNullOrEmpty(promotionDto.PromotionName)) 
                return BadRequest("You must provide a name for the promotion");
            if (promotionDto.Quantity <= 0) 
                return BadRequest("You must provide a min quantity for the promotion");
            if(promotionDto.PromotionPrice <= 0)
                return BadRequest("You must provide a price offer for the promotion");
            return BadRequest("Please provide valid promotion details");
        }


        [HttpPost("test")]
        public IActionResult Test(Object jsonResult)
        {
            dynamic reqBody = JObject.Parse(jsonResult.ToString());       
       //     List<int> productIds = reqBody.productIds.ToObject<IList<int>>();
            List<Product> products = _inventoryService.GetProducts(reqBody.productIds.ToObject<IList<int>>());
            var promos = _service.ApplyPromotions(1,products);
            var dtos = _mapper.Map<IList<ProductSale>, IList<ProductSaleDto>>(promos);
            return Ok(dtos);
        }
    }
        
}
